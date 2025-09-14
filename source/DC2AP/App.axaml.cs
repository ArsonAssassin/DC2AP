using Archipelago.Core;
using Archipelago.Core.AvaloniaGUI.Models;
using Archipelago.Core.AvaloniaGUI.ViewModels;
using Archipelago.Core.AvaloniaGUI.Views;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using DC2AP.Models;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text.Json;
using System.Timers;

namespace DC2AP;

public partial class App : Application
{
    static MainWindowViewModel Context;
    public static ArchipelagoClient Client { get; set; }
    public static List<DarkCloud2Item> ItemList { get; set; }
    public static List<Enemy> EnemyList { get; set; }
    public static List<QuestId> QuestList { get; set; }
    public static List<Dungeon> DungeonList { get; set; }
    public static PlayerState CurrentPlayerState;
    private static readonly object _lockObject = new object();
    Timer updateTimer = new Timer(TimeSpan.FromSeconds(10));
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Context = new MainWindowViewModel();
        Context.ConnectClicked += Context_ConnectClicked;
        Context.CommandReceived += (e, a) =>
        {
            Client?.SendMessage(a.Command);
        };
        Context.ConnectButtonEnabled = true;
        updateTimer.Elapsed += GameUpdate;
        updateTimer.Start();
    }

    private void GameUpdate(object? sender, ElapsedEventArgs e)
    {
        if(! (Client?.IsConnected ?? false)) return;
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


    private async void Context_ConnectClicked(object? sender, ConnectClickedEventArgs e)
    {
        Context.ConnectButtonEnabled = false;
        Log.Logger.Information("Connecting...");
        if (Client != null)
        {
            Client.Connected -= OnConnected;
            Client.Disconnected -= OnDisconnected;
            Client.ItemReceived -= Client_ItemReceived;
            Client.MessageReceived -= Client_MessageReceived;
            Client.CancelMonitors();
        }
        var client = new GenericGameClient("pcsx2-qt");
        var connected = client.Connect();
        if (!connected)
        {
            Log.Logger.Error("PCSX2 not running, open PCSX2 and load your game before connecting!");
            Context.ConnectButtonEnabled = true;
            return;
        }

        Client = new ArchipelagoClient(client);

        Memory.GlobalOffset = Memory.GetPCSX2Offset();

        var palAddress = Memory.ReadInt(0x003694D0);
        var usAddress = Memory.ReadInt(0x00364BD0);
        var gameVersion = palAddress == 1701667175 ? "PAL" : usAddress == 1701667175 ? "US" : "";
        if (string.IsNullOrWhiteSpace(gameVersion))
        {
            Log.Logger.Information("Dark cloud 2 is not loaded, please load the game and try again.");
            Context.ConnectButtonEnabled = true;
            return;
        }
        else if (gameVersion == "PAL")
        {
            Log.Logger.Information("You have loaded the Dark Chronicle (PAL). Only Dark Cloud 2 (US) is supported");
            Context.ConnectButtonEnabled = true;
            return;
        }


        Client.Connected += OnConnected;
        Client.Disconnected += OnDisconnected;

        await Client.Connect(e.Host, "Dark Cloud 2");

        PopulateLists();
        CurrentPlayerState = new PlayerState();
        Client.ItemReceived += Client_ItemReceived;
        Client.MessageReceived += Client_MessageReceived;

        await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null);
        CurrentPlayerState.UpdateInventory();
        UpdatePlayerState();


        CurrentPlayerState.InventoryChanged += async (obj, args) =>
        {
            Log.Logger.Information($"Inventory changed: {JsonConvert.SerializeObject(args, Formatting.Indented)}");
            if (!args.IsArchipelagoUpdate)
            {
                foreach (DarkCloud2Item item in args.NewItems)
                {
                    if (item.IsProgression)
                    {
                        Helpers.RemoveItem(item, CurrentPlayerState);
                        var itemId = item.Id;
                        var location = Helpers.GetLocationFromProgressionItem(itemId);
                        if (location != -1)
                        {
                            Client.SendLocation(new Location() { Id = location });
                        }
                    }
                }
            }
        };
        CurrentPlayerState.PropertyChanged += (obj, args) =>
        {
            Log.Logger.Information($"Player State changed: {JsonConvert.SerializeObject(args, Formatting.Indented)}");
        };

        var enemies = Helpers.ReadEnemies();
        var locations = Helpers.GetLocations();
        Client.MonitorLocations(locations);

        var goalLocation = (Location)locations.First(x => x.Name.ToLower().Contains("chapter 5 complete"));        
        Archipelago.Core.Util.Memory.MonitorAddressForAction<byte>(goalLocation.Address, () => Client.SendGoalCompletion(), (o) => { return o > 4; });

        if (Client.Options.ContainsKey("enable_enemy_randomiser") && ((JsonElement)Client.Options["enable_enemy_randomiser"]).Deserialize<int>() > 0)
        {
            Helpers.ShuffleEnemies(enemies);
        }
        //Client.MonitorLocations(bossLocations);

        Context.ConnectButtonEnabled = true;
    }
    static void PopulateLists()
    {
        Log.Logger.Debug("Building Item List");
        ItemList = Helpers.GetItemIds();
        Log.Logger.Debug("Building Quest List");
        QuestList = Helpers.GetQuestIds();
        Log.Logger.Debug("Building Dungeon List");
        DungeonList = PopulateDungeons();
        Log.Logger.Debug("Building Enemy List");
        EnemyList = Helpers.ReadEnemies();
    }
    static void UpdatePlayerState()
    {
        if (CurrentPlayerState == null) CurrentPlayerState = new PlayerState();
        CurrentPlayerState.CurrentFloor = Memory.ReadByte(Addresses.CurrentFloor);
        CurrentPlayerState.CurrentDungeon = Memory.ReadByte(Addresses.CurrentDungeon);
        CurrentPlayerState.Gilda = Memory.ReadInt(Addresses.PlayerGilda);
        CurrentPlayerState.MedalCount = Memory.ReadShort(Addresses.PlayerMedals);
        CurrentPlayerState.UpdateInventory();
    }
    public static List<Dungeon> PopulateDungeons()
    {
        List<Dungeon> dungeons = Helpers.GetDungeons();

        var currentAddress = Addresses.DungeonStartAddress;

        foreach (var dungeon in dungeons)
        {
            dungeon.Floors = new List<Floor>();
            for (int i = 0; i < dungeon.FloorCount; i++)
            {
                Floor floor = Helpers.ReadFloor(currentAddress);
                currentAddress += 0x0000014;
                dungeon.Floors.Add(floor);
                Log.Logger.Debug(JsonConvert.SerializeObject(floor, Formatting.Indented));
            }
            currentAddress += 0x0000014;
        }
        return dungeons;
    }
    private void Client_ItemReceived(object? sender, ItemReceivedEventArgs e)
    {
        e.Item.Id = Helpers.ToGameId((int)e.Item.Id);
        if (e.Item.Id <= 428)
        {
            e.Item.Name = ItemList.First(x => x.Id == e.Item.Id).Name;
            Helpers.AddItem(e.Item, CurrentPlayerState);
        }
        else
        {
            //An event was completed
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
