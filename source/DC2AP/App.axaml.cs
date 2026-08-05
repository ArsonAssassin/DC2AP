using Archipelago.Core;
using Archipelago.Core.AvaloniaGUI.Models;
using Archipelago.Core.AvaloniaGUI.ViewModels;
using Archipelago.Core.AvaloniaGUI.Views;
using Archipelago.Core.Helpers;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DC2AP.Helpers;
using DC2AP.Models;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using static DC2AP.Helpers.OptionsHelper;

namespace DC2AP;

public partial class App : Application
{
    static MainWindowViewModel Context;
    public static ArchipelagoClient Client { get; set; }
    public static PlayerState CurrentPlayerState;
    private static readonly object _lockObject = new object();
    private NotificationHelper NotificationHelper;
    private OptionsHelper OptionsHelper;
    Timer updateTimer = new Timer(TimeSpan.FromSeconds(10));
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Context = new MainWindowViewModel() { ConnectButtonEnabled = true };
        Context.ConnectClicked += Context_ConnectClicked;
        Context.CommandReceived += (_, a) => Client?.SendMessage(a.Command);

        updateTimer.Elapsed += GameUpdate;
        updateTimer.Start();
    }

    private void GameUpdate(object? sender, ElapsedEventArgs e)
    {
        if (!(Client?.IsConnected ?? false)) return;
        UpdatePlayerState();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Context
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainWindow
            {
                DataContext = Context
            };
        }
        base.OnFrameworkInitializationCompleted();
    }

    private void UnsubscribeClientEvents()
    {
        if (Client == null) return;

        Client.Connected -= OnConnected;
        Client.Disconnected -= OnDisconnected;
        Client.ItemManager.ItemReceived -= Client_ItemReceived;
        Client.MessageReceived -= Client_MessageReceived;
        Client.LocationManager.CancelMonitors();
    }
    private bool TryConnectToGame()
    {
        var client = new GameClient("pcsx2");
        if (!client.Connect())
        {
            Log.Logger.Error("PCSX2 not running, open PCSX2 and load your game before connecting!");
            return false;
        }

        Client = new ArchipelagoClient(client);
        Memory.GlobalOffset = Memory.GetPCSX2Offset();
        NotificationHelper = new NotificationHelper();
        NotificationHelper.Install();
        var options = new List<OptionRow>
        {
            new()
        {
            Label          = "Test Option",
            ButtonCount    = 2,
            GetInitialValue = () => 0,
            OnChanged      = val => { },
        }
        };
        OptionsHelper = new OptionsHelper(options);
        OptionsHelper.Start();
        return true;
    }
    private async Task ConnectToArchipelago(ConnectClickedEventArgs e)
    {
        if (Client == null) return;

        Client.Connected += OnConnected;
        Client.Disconnected += OnDisconnected;
        Client.MessageReceived += Client_MessageReceived;

        await Client.Connect(e.Host, "Dark Cloud 2");

        GeneralHelpers.PopulateLists();
        CurrentPlayerState = new PlayerState();

        await Client.Login(e.Slot, string.IsNullOrWhiteSpace(e.Password) ? null : e.Password);
        Client.ItemManager!.Initialize();
        Client.ItemManager.ItemReceived += Client_ItemReceived;
        await Client.ItemManager.ReceiveReady(Client.CurrentSession);
        CurrentPlayerState.UpdateInventory();
        UpdatePlayerState();

        SetupPlayerStateHandlers();
        SetupLocationMonitoring();
    }
    private async void Context_ConnectClicked(object? sender, ConnectClickedEventArgs e)
    {
        Context.ConnectButtonEnabled = false;
        Log.Logger.Information("Connecting...");
        UnsubscribeClientEvents();

        if (!TryConnectToGame())
        {
            Context.ConnectButtonEnabled = true;
            return;
        }

        if (!ValidateGameVersion())
            return;

        await ConnectToArchipelago(e);

        Context.ConnectButtonEnabled = true;
    }
    private void SetupPlayerStateHandlers()
    {
        if (CurrentPlayerState == null) return;

        CurrentPlayerState.InventoryChanged += (_, args) =>
        {
            Log.Logger.Information($"Inventory changed: {JsonConvert.SerializeObject(args, Formatting.Indented)}");

            if (args.IsArchipelagoUpdate) return;

            foreach (var item in args.NewItems.Where(i => GeneralHelpers.IsProgressionItem(i.ItemId)))
            {
                GeneralHelpers.RemoveItem(item, CurrentPlayerState);
                var location = GeneralHelpers.GetLocationFromProgressionItem((int)item.Id);

                if (location != -1)
                {
                    Client?.SendLocationAsync(new Location { Id = (int)location });
                }
            }
        };

        CurrentPlayerState.PropertyChanged += (_, args) =>
        {
            Log.Logger.Information($"Player State changed: {JsonConvert.SerializeObject(args, Formatting.Indented)}");
        };
    }
    private void SetupLocationMonitoring()
    {
        if (Client == null) return;

        var enemies = GeneralHelpers.ReadEnemies();
        var locations = GeneralHelpers.GetLocations();
        Client.LocationManager.MonitorLocationsAsync(Client.CurrentSession, locations);

        var goalLocation = (Location)locations.First(x => x.Name.Contains("chapter 5 complete", StringComparison.OrdinalIgnoreCase));
        Memory.MonitorAddressForAction<byte>(
            goalLocation.Address,
            () => Client.SendGoalCompletion(),
            value => value > 4
        );

        if (Client?.Options.ContainsKey("enable_enemy_randomiser") == true
            && ((JsonElement)Client.Options["enable_enemy_randomiser"]).Deserialize<int>() > 0)
        {
            GeneralHelpers.ShuffleEnemies(enemies);
        }
    }
    private static bool ValidateGameVersion()
    {
        var palAddress = Memory.ReadInt(0x003694D0);
        var usAddress = Memory.ReadInt(0x00364BD0);
        var gameVersion = palAddress == 1701667175 ? "PAL" : usAddress == 1701667175 ? "US" : "";
        if (string.IsNullOrWhiteSpace(gameVersion))
        {
            Log.Logger.Information("Dark cloud 2 is not loaded, please load the game and try again.");
            Context.ConnectButtonEnabled = true;
            return false;
        }
        else if (gameVersion == "PAL")
        {
            Log.Logger.Information("You have loaded Dark Chronicle (PAL). Only Dark Cloud 2 (US) is supported");
            Context.ConnectButtonEnabled = true;
            return false;
        }

        return true;
    }


    static void UpdatePlayerState()
    {
        CurrentPlayerState ??= new PlayerState();
        CurrentPlayerState.CurrentFloor = Memory.ReadByte(Addresses.CurrentFloor);
        CurrentPlayerState.CurrentDungeon = Memory.ReadByte(Addresses.CurrentDungeon);
        CurrentPlayerState.Gilda = Memory.ReadInt(Addresses.PlayerGilda);
        CurrentPlayerState.MedalCount = Memory.ReadShort(Addresses.PlayerMedals);
        CurrentPlayerState.UpdateInventory();
    }

    private void Client_ItemReceived(object? sender, ItemReceivedEventArgs e)
    {
        e.Item.Id = GeneralHelpers.ToGameId((int)e.Item.Id);
        if (e.Item.Id <= 428)
        {
            var newItem = GeneralHelpers.DefaultItems.FirstOrDefault(x => x.Id == e.Item.Id);
            if (newItem == null)
            {
                Log.Logger.Error($"Could not find default item stats for item with id: {e.Item.Id}");
                return;
            }
            NotificationHelper.ShowNotification("Received " + newItem.Name);
            GeneralHelpers.AddItem(newItem, CurrentPlayerState, 1, true);
        }
        else if (e.Item.Id <= 1999 && e.Item.Id >= 1000)
        {
            //An event was completed
        }
        else if (e.Item.Id <= 2999 && e.Item.Id >= 2000)
        {
            RxApp.MainThreadScheduler.ScheduleAsync(async (s, t) =>
            {
                // Reward item
                var pack = GeneralHelpers.GetRewardPack(e.Item.Id);
                foreach (var (itemId, quantity) in pack)
                {
                    var newItem = GeneralHelpers.DefaultItems.FirstOrDefault(x => x.Id == itemId);
                    if (newItem == null)
                    {
                        Log.Logger.Error($"Could not find default item stats for item with id: {itemId}");
                        continue;
                    }
                    GeneralHelpers.AddItem(newItem, CurrentPlayerState, quantity, true);
                    await Task.Delay(100);
                }
            });
        }
        else
        {
            Log.Logger.Error($"Could not find default item stats for item with id: {e.Item.Id}");
        }
    }

    private void Client_MessageReceived(object? sender, Archipelago.Core.Models.MessageReceivedEventArgs e)
    {
        if (e.Message.Parts.Any(x => x.Text == "[Hint]: "))
        {
            LogHint(e.Message);
        }
        Log.Logger.Information(JsonConvert.SerializeObject(e.Message));
    }
    private static void LogItem(Item item)
    {
        var messageToLog = new LogListItem(new List<TextSpan>()
            {
                new TextSpan(){Text = $"[{item.Id.ToString()}] -", TextColor = new SolidColorBrush(Color.FromRgb(255, 255, 255))},
                new TextSpan(){Text = $"{item.Name}", TextColor = new SolidColorBrush(Color.FromRgb(200, 255, 200))}
            });
        lock (_lockObject)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                Context.ItemList.Add(messageToLog);
            });
        }
    }
    private static void LogHint(LogMessage message)
    {
        var newMessage = message.Parts.Select(x => x.Text);

        if (Context.HintList.Any(x => x.TextSpans.Select(y => y.Text) == newMessage))
        {
            return; //Hint already in list
        }
        List<TextSpan> spans = new List<TextSpan>();
        foreach (var part in message.Parts)
        {
            spans.Add(new TextSpan() { Text = part.Text, TextColor = new SolidColorBrush(Color.FromRgb(part.Color.R, part.Color.G, part.Color.B)) });
        }
        lock (_lockObject)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                Context.HintList.Add(new LogListItem(spans));
            });
        }
    }
    private static void OnConnected(object sender, EventArgs args)
    {
        Log.Logger.Information("Connected to Archipelago");
        Log.Logger.Information($"Playing {Client.CurrentSession.ConnectionInfo.Game} as {Client.CurrentSession.Players.GetPlayerName(Client.CurrentSession.ConnectionInfo.Slot)}");
    }

    private static void OnDisconnected(object sender, EventArgs args)
    {
        Log.Logger.Information("Disconnected from Archipelago");
    }
}
