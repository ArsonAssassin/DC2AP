using Archipelago.Core;
using Archipelago.Core.GameClients;
using Archipelago.Core.MauiGUI;
using Archipelago.Core.MauiGUI.Models;
using Archipelago.Core.MauiGUI.ViewModels;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using DC2AP.Models;
using Newtonsoft.Json;
using Serilog;
using System.Reflection;

namespace DC2AP
{
    public partial class App : Application
    {
        static MainPageViewModel Context;
        public static ArchipelagoClient Client { get; set; }
        public static List<DarkCloud2Item> ItemList { get; set; }
        public static List<Enemy> EnemyList { get; set; }
        public static List<QuestId> QuestList { get; set; }
        public static List<Dungeon> DungeonList { get; set; }
        public static Models.GameState CurrentGameState = new Models.GameState();
        public static PlayerState CurrentPlayerState = new PlayerState();
        private static readonly object _lockObject = new object();
        public App()
        {
            InitializeComponent();
            var options = new GuiDesignOptions
            {
                BackgroundColor = Color.FromArgb("FF34A7E2"),
                ButtonColor = Color.FromArgb("FFB87E45"),
                ButtonTextColor = Color.FromArgb("FF000000"),
                Title = "DC2AP - Dark Cloud 2 Archipelago",

            };

            Context = new MainPageViewModel(options);
            Context.ConnectClicked += Context_ConnectClicked;
            Context.CommandReceived += (e, a) =>
            {
                Client?.SendMessage(a.Command);
            };
            MainPage = new MainPage(Context);
            Context.ConnectButtonEnabled = true;
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
            PCSX2Client client = new PCSX2Client();
            var connected = client.Connect();
            if (!connected)
            {
                Log.Logger.Error("PCSX2 not running, open PCSX2 and load your game before connecting!");
                Context.ConnectButtonEnabled = true;
                return;
            }

            Client = new ArchipelagoClient(client);

            var palAddress = Memory.ReadInt(0x203694D0);
            var usAddress = Memory.ReadInt(0x20364BD0);
            var gameVersion = palAddress == 1701667175 ? "PAL" : usAddress == 1701667175 ? "US" : "";
            if (string.IsNullOrWhiteSpace(gameVersion))
            {
                Log.Logger.Information("Dark cloud 2 is not loaded, please load the game and try again.");
                Context.ConnectButtonEnabled = true;
                return;
            }
            else if(gameVersion == "PAL")
            {
                Log.Logger.Information("You have loaded the Dark Chronicle (PAL). Only Dark Cloud 2 (US) is supported");
                Context.ConnectButtonEnabled = true;
                return;
            }


            Client.Connected += OnConnected;
            Client.Disconnected += OnDisconnected;

            await Client.Connect(e.Host, "Dark Cloud 2");

            Client.ItemReceived += Client_ItemReceived;
            Client.MessageReceived += Client_MessageReceived;

            await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null);


            var enemies = Helpers.ReadEnemies();
            //var bossLocations = Helpers.GetBossFlagLocations();

            //var goalLocation = bossLocations.First(x => x.Name.Contains("Lord of Cinder"));
            //Archipelago.Core.Util.Memory.MonitorAddressBitForAction(goalLocation.Address, goalLocation.AddressBit, () => Client.SendGoalCompletion());

            //Client.MonitorLocations(bossLocations);

            Context.ConnectButtonEnabled = true;
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
                new TextSpan(){Text = $"[{item.Id.ToString()}] -", TextColor = Color.FromRgb(255, 255, 255)},
                new TextSpan(){Text = $"{item.Name}", TextColor = Color.FromRgb(200, 255, 200)},
                new TextSpan(){Text = $"x{item.Quantity.ToString()}", TextColor = Color.FromRgb(200, 255, 200)}
            });
            lock (_lockObject)
            {
                Application.Current.Dispatcher.DispatchAsync(() =>
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
                spans.Add(new TextSpan() { Text = part.Text, TextColor = Color.FromRgb(part.Color.R, part.Color.G, part.Color.B) });
            }
            lock (_lockObject)
            {
                Application.Current.Dispatcher.DispatchAsync(() =>
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
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                window.Title = "DC2AP - Dark Cloud 2 Archipelago Randomizer";

            }
            window.Width = 600;

            return window;
        }
    }
}
