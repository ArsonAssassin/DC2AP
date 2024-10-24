using Archipelago.Core;
using Archipelago.Core.Models;
using Archipelago.Core.GUI;
using Archipelago.Core.Util;
using Archipelago.PCSX2;
using Serilog;
using System.Reflection;
using DC2AP.Models;
using Newtonsoft.Json;
using System.Collections;

namespace DC2AP
{
    internal static class Program
    {
        public static MainForm MainForm;

        public static string GameVersion { get; set; }
        public static List<ItemId> ItemList { get; set; }
        public static List<Enemy> EnemyList { get; set; }
        public static List<QuestId> QuestList { get; set; }
        public static List<Dungeon> DungeonList { get; set; }
        public static Models.GameState CurrentGameState = new Models.GameState();
        public static PlayerState CurrentPlayerState = new PlayerState();
        public static ArchipelagoClient Client { get; set; }


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            var options = new GuiDesignOptions
            {

            };
            MainForm = new MainForm(options);
            MainForm.ConnectClicked += MainForm_ConnectClicked;
            MainForm.Load += MainForm_Load;
            Application.Run(MainForm);
        }
        private static async Task MainLoop()
        {
            Console.WriteLine("Beginning main loop.");
            while (true)
            {


                if (Memory.ReadByte(Addresses.Instance.CurrentFloor) == 0)
                {
                    Addresses.Instance.PreviousFloor = 200;
                }
                else
                {
                    if (Addresses.Instance.CurrentFloor != Addresses.Instance.PreviousFloor)
                    {
                        Console.WriteLine("Moved to new floor");
                        Thread.Sleep(6000);

                        var currentAddress = Addresses.Instance.DungeonAreaChestAddress[Memory.ReadByte(Addresses.Instance.CurrentDungeon)] + 0x0000005C;
                        Task.Factory.StartNew(() =>
                        {
                            while (true)
                            {
                                Thread.Sleep(10);
                                if (Memory.ReadByte(Addresses.Instance.CurrentFloor) == 0 || Memory.ReadByte(Addresses.Instance.DungeonCheckAddress) > 2)
                                {
                                    Console.WriteLine("Exited dungeon");
                                    break;
                                }
                            }
                        });
                        Thread.Sleep(1000);
                        // Ensures Map and Magic Crystal are available

                        //Console.WriteLine("Map spawned on first chest");
                        //Memory.Write(currentAddress, (ushort)ItemList.First(x => x.Name.ToLower() == "map").Id);
                        //currentAddress += 0x00000070;
                        //Console.WriteLine("Magic crystal spawned on second chest");
                        //Memory.Write(currentAddress, (ushort)ItemList.First(x => x.Name.ToLower() == "magic crystal").Id);
                        //currentAddress += 0x0000006C;

                        var chests = Helpers.ReadChests();
                        Addresses.Instance.PreviousFloor = Addresses.Instance.CurrentFloor;
                    }
                }

                //Handle exiting the game
                var exitFlagCheck = 1701667175;
                if (Memory.ReadInt(Addresses.Instance.CurrentExitFlag) != exitFlagCheck)
                {
                    Thread.Sleep(1000);
                    if (Memory.ReadInt(Addresses.Instance.CurrentExitFlag) != exitFlagCheck)
                    {
                        System.Environment.Exit(0);
                    }
                }

                UpdateGameState();
                UpdatePlayerState();


                Thread.Sleep(1);
            }
        }
        private static void MainForm_Load(object? sender, EventArgs e)
        {
            Log.Logger.Information("DC2AP - Dark Cloud 2 Archipelago Randomizer -- By ArsonAssassin --");
        }

        private static async void MainForm_ConnectClicked(object? sender, ConnectClickedEventArgs e)
        {
            PCSX2Client client = new PCSX2Client();
            var pcsx2Connected = client.Connect();
            if (!pcsx2Connected)
            {
                Log.Logger.Information("An error occurred whilst connecting to PCSX2, please ensure the application is open and the game is loaded.");               
            }
            Console.WriteLine($"Connecting to Archipelago");
            Client = new ArchipelagoClient(client);
            var palAddress = Memory.ReadInt(0x203694D0);
            var usAddress = Memory.ReadInt(0x20364BD0);
            GameVersion = palAddress == 1701667175 ? "PAL" : usAddress == 1701667175 ? "US" : "";
            if (string.IsNullOrWhiteSpace(GameVersion))
            {
                Log.Logger.Information("Dark cloud 2 is not loaded, please load the game and try again.");
            }
            Console.WriteLine($"Connected to Dark Cloud 2 ({GameVersion})");

            await Client.Connect(e.Host, "Dark Cloud 2");
            await Client.Login(e.Slot, e.Password);

            PopulateLists();
            UpdateGameState();
            UpdatePlayerState();

            var locations = Helpers.GetLocations();
            Client.PopulateLocations(locations);
            Client.ItemReceived += (e, args) =>
            {
                args.Item.Id = Helpers.ToGameId((int)args.Item.Id);
                if (args.Item.Id <= 428)
                {
                    args.Item.Name = ItemList.First(x => x.Id == args.Item.Id).Name;
                    Helpers.AddItem(args.Item, CurrentPlayerState);
                }
                else
                {
                    //An event was completed
                }
            };

            CurrentGameState.PropertyChanged += (obj, args) =>
            {
                Console.WriteLine($"Game State changed: {JsonConvert.SerializeObject(args, Formatting.Indented)}");
            };
            CurrentPlayerState.InventoryChanged += async (obj, args) =>
            {
                Console.WriteLine($"Inventory changed: {JsonConvert.SerializeObject(args, Formatting.Indented)}");
                if (!args.IsArchipelagoUpdate)
                {
                    foreach (Item item in args.NewItems)
                    {
                        if (item.IsProgression)
                        {
                            Helpers.RemoveAllItem(item, CurrentPlayerState);
                            var itemId = item.Id;
                            var location = Helpers.GetLocationFromProgressionItem(itemId);
                            if (location != -1)
                            {
                                var locationId = Client.CurrentSession.Locations.AllLocations.FirstOrDefault(x => x == location);
                                Client.SendLocation(new Location() { Id = (int)locationId });
                            }
                        }
                    }
                }
            };
            CurrentPlayerState.PropertyChanged += (obj, args) =>
            {
                Console.WriteLine($"Player State changed: {JsonConvert.SerializeObject(args, Formatting.Indented)}");
            };


        }

        static void PopulateLists()
        {
            Console.WriteLine("Building Item List");
            ItemList = Helpers.GetItemIds();
            Console.WriteLine("Building Quest List");
            QuestList = Helpers.GetQuestIds();
            Console.WriteLine("Building Dungeon List");
            DungeonList = PopulateDungeons();
            Console.WriteLine("Building Enemy List");
            EnemyList = Helpers.ReadEnemies();
        }
        static void UpdateGameState()
        {
            CurrentGameState.CurrentFloor = Memory.ReadByte(Addresses.Instance.CurrentFloor);
            CurrentGameState.CurrentDungeon = Memory.ReadByte(Addresses.Instance.CurrentDungeon);
        }
        static void UpdatePlayerState()
        {
            CurrentPlayerState.Gilda = Memory.ReadInt(Addresses.Instance.PlayerGilda);
            CurrentPlayerState.MedalCount = Memory.ReadShort(Addresses.Instance.PlayerMedals);
            var tempInv = ReadInventory();
            for (int i = 0; i < tempInv.Count; i++)
            {
                if (tempInv[i].Id != CurrentPlayerState.Inventory[i].Id || tempInv[i].Quantity != CurrentPlayerState.Inventory[i].Quantity)
                {
                    CurrentPlayerState.Inventory[i] = tempInv[i];
                }
            }
        }

       
        public static List<Item> ReadInventory(bool debug = false)
        {
            List<Item> inventory = new List<Item>();

            var startAddress = Addresses.Instance.InventoryStartAddress;

            for (int i = 0; i < 144; i++)
            {
                Item item = new Item();

                var itemId = Memory.ReadShort(startAddress);
                item.Id = itemId;
                var itemQuantityAddress = startAddress + 0x0000000E;
                var itemQuantity = Memory.ReadShort(itemQuantityAddress);
                item.Quantity = itemQuantity;
                item.Name = ItemList.First(x => x.Id == item.Id).Name;
                item.IsProgression = ItemList.FirstOrDefault(x => x.Id == itemId).isProgression;
                if (debug) Console.WriteLine($"Inventory slot {i}: {item.Name}, {item.Id} x {item.Quantity}");
                startAddress += 0x0000006C;
                inventory.Add(item);
            }
            return inventory;
        }

        public static List<Dungeon> PopulateDungeons(bool debug = false)
        {
            List<Dungeon> dungeons = Helpers.GetDungeons();

            var currentAddress = Addresses.Instance.DungeonStartAddress;

            foreach (var dungeon in dungeons)
            {
                dungeon.Floors = new List<Floor>();
                for (int i = 0; i < dungeon.FloorCount; i++)
                {
                    Floor floor = Helpers.ReadFloor(currentAddress);
                    currentAddress += 0x0000014;
                    dungeon.Floors.Add(floor);
                    if (debug) Console.WriteLine(JsonConvert.SerializeObject(floor, Formatting.Indented));
                }
                currentAddress += 0x0000014;
            }
            return dungeons;
        }

    }
}