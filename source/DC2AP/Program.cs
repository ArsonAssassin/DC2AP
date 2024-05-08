using DC2AP.Archipelago;
using DC2AP.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace DC2AP
{
    public static class Program
    {
        public static string GameVersion { get; set; }
        public static List<ItemId> ItemList { get; set; }
        public static List<Enemy> EnemyList { get; set; }
        public static List<QuestId> QuestList { get; set; }
        public static List<Dungeon> DungeonList { get; set; }
        public static bool IsConnected = false;
        public static GameState CurrentGameState = new GameState();
        public static PlayerState CurrentPlayerState = new PlayerState();
        public static ArchipelagoClient Client { get; set; }
        public static async Task Main()
        {
            Console.SetBufferSize(Console.BufferWidth, 32766);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Console.WriteLine("DC2AP - Dark Cloud 2 Archipelago Randomizer");

            await Initialise();

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

                        var chests = ReadChests();
                        Addresses.Instance.PreviousFloor = Addresses.Instance.CurrentFloor;
                    }
                }

                //Handle exiting the game

                if (Memory.ReadInt(Addresses.Instance.CurrentExitFlag) != Addresses.Instance.exitFlagCheck)
                {
                    Thread.Sleep(1000);
                    if (Memory.ReadInt(Addresses.Instance.CurrentExitFlag) != Addresses.Instance.exitFlagCheck)
                    {
                        System.Environment.Exit(0);
                    }
                }

                UpdateGameState();
                UpdatePlayerState();

                HandleInputCommands();
                Thread.Sleep(1);
            }
        }

        private static async Task Initialise()
        {
            IsConnected = await ConnectAsync();
            
            PopulateLists();
            UpdateGameState();
            UpdatePlayerState();

            CurrentGameState.PropertyChanged += (obj, args) =>
            {
                Console.WriteLine($"Game State changed: {JsonConvert.SerializeObject(args, Formatting.Indented)}");
            };
            CurrentPlayerState.InventoryChanged += (obj, args) =>
            {
                Console.WriteLine($"Inventory changed: {JsonConvert.SerializeObject(args, Formatting.Indented)}");
            };
            CurrentPlayerState.PropertyChanged += (obj, args) =>
            {
                Console.WriteLine($"Player State changed: {JsonConvert.SerializeObject(args, Formatting.Indented)}");
            };
        }

        private static void HandleInputCommands()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.F1)
                {
                    var freeSlot = CurrentPlayerState.GetFirstSlot(268);
                    Helpers.WriteItem(new Item() { Id = 268, Quantity = 20 }, Helpers.GetItemSlotAddress(freeSlot));
                }
                if (key.Key == ConsoleKey.F2)
                {
                    var freeSlot = CurrentPlayerState.GetFirstSlot(0);
                    Console.WriteLine("Enter item name:");
                    var itemName = Console.ReadLine();
                    Helpers.WriteItem(new Item() { Id = ItemList.First(x => x.Name.ToLower() == itemName).Id, Quantity = 1 }, Helpers.GetItemSlotAddress(freeSlot));
                }
            }
        }

        private static List<Chest> ReadChests()
        {
            List<Chest> chests = new List<Chest>();
            var chestStartAddress = Addresses.Instance.DungeonAreaChestAddress[Memory.ReadByte(Addresses.Instance.CurrentDungeon)];
            var currentChestAddress = chestStartAddress;
            var chestId = 0;
            while (chestId != 1)
            {
                chestId = Memory.ReadByte(currentChestAddress);
                var chest = ReadChest(currentChestAddress, chestId >= 128);
                chests.Add(chest);
                currentChestAddress += 0x00000070;
            }
            Console.WriteLine("End of chests");
            return chests;
        }

        static async Task<bool> ConnectAsync()
        {
            Console.WriteLine("Connecting to PCSX2");
            var pid = Memory.PCSX2_PROCESSID;
            if (pid == 0)
            {
                Console.WriteLine("PCSX2 not found.");
                Console.WriteLine("Press any key to exit.");
                Console.Read();
                System.Environment.Exit(0);
                return false;
            }
            GameVersion = Memory.ReadInt(0x203694D0) == 1701667175 ? "PAL" : Memory.ReadInt(0x20364BD0) == 1701667175 ? "US" : "";
            if (string.IsNullOrWhiteSpace(GameVersion))
            {
                Console.WriteLine("Dark cloud 2 is not loaded, please load the game and try again.");
                Console.WriteLine("Press any key to exit.");
                Console.Read();
                System.Environment.Exit(0);
                return false;
            }
            Console.WriteLine($"Connected to Dark Cloud 2 ({GameVersion})");

            Console.WriteLine($"Connecting to Archipelago");
            Client = new ArchipelagoClient();
            await Client.Connect("localhost:38281");
            await Client.Login("Player1");
            Client.ItemReceived += (e, args) =>
            {
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
            return true;

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
            EnemyList = ReadEnemies();
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
        static Chest ReadChest(int startAddress, bool isDouble = false)
        {
            Chest chest = new Chest() { IsDoubleChest = isDouble };
            var currentAddress = startAddress + 0x00000004;
            chest.Item1 = Memory.ReadShort(currentAddress);
            currentAddress += Addresses.Instance.ShortOffset;
            if (isDouble) chest.Item2 = Memory.ReadShort(currentAddress);
            currentAddress += Addresses.Instance.ShortOffset;
            chest.Quantity1 = Memory.ReadShort(currentAddress);
            currentAddress += Addresses.Instance.ShortOffset;
            if (isDouble) chest.Quantity2 = Memory.ReadShort(currentAddress);
            return chest;
        }
        static void AddChestItem(int startAddress, int id, int quantity)
        {
            startAddress += 0x00000004;
            Console.WriteLine($"Setting Chest contents to {id}");
            Memory.Write(startAddress, BitConverter.GetBytes(id));
            startAddress += 0x00000004;
            Memory.Write(startAddress, BitConverter.GetBytes(quantity));
            Console.WriteLine("Added item!");
        }
        static void AddDoubleChestItems(int startAddress, int id1, int quantity1, int id2, int quantity2)
        {
            startAddress += 0x00000004;
            var currentItem = Memory.ReadByte(startAddress);
            Console.WriteLine($"replacing {currentItem} with {id1}");
            Memory.Write(startAddress, BitConverter.GetBytes(id1));
            startAddress += Addresses.Instance.ShortOffset;
            var currentItem2 = Memory.ReadByte(startAddress);
            Console.WriteLine($"replacing {currentItem2} with {id2}");
            Memory.Write(startAddress, BitConverter.GetBytes(id2));
            startAddress += Addresses.Instance.ShortOffset;
            Memory.Write(startAddress, BitConverter.GetBytes(quantity1));
            startAddress += Addresses.Instance.ShortOffset;
            Memory.Write(startAddress, BitConverter.GetBytes(quantity2));
        }

        static async Task MonitorAddress(int address)
        {
            var initialValue = Memory.ReadByte(address);
            var currentValue = initialValue;
            while (initialValue == currentValue)
            {
                currentValue = Memory.ReadByte(address);
                Thread.Sleep(10);
            }
            Console.WriteLine($"Memory value changed at address {address.ToString("X8")}");
        }
        static async Task MonitorAddressRange(int address, int length)
        {
            var initialValue = Memory.ReadString(address, length);
            var currentValue = initialValue;
            Console.WriteLine($"Monitoring address {address.ToString("X8")} with initial value {initialValue}");
            while (initialValue == currentValue)
            {
                currentValue = Memory.ReadString(address, length);
                Thread.Sleep(10);
            }
            Console.WriteLine($"Memory value changed at address {address.ToString("X8")} from {initialValue} to {currentValue}");
        }
        static List<Enemy> ReadEnemies(bool debug = false)
        {

            var expMultipler = 1;

            List<Enemy> enemies = new List<Enemy>();
            var currentAddress = Addresses.Instance.EnemyStartAddress;
            currentAddress += 0x00000004;
            for (int i = 0; i < 280; i++)
            {
                Enemy enemy = new Enemy();
                enemy.Name = Memory.ReadString(currentAddress, 32);
                currentAddress += 0x00000020;
                enemy.ModelAI = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 32));
                currentAddress += 0x00000020;
                var modelType = Memory.ReadInt(currentAddress).ToString();
                enemy.ModelType = Helpers.GetModelType(modelType);
                currentAddress += 0x00000004;
                enemy.Sound = Memory.ReadInt(currentAddress).ToString();
                currentAddress += 0x00000004;
                enemy.Unknown1 = Memory.ReadInt(currentAddress).ToString();
                currentAddress += 0x00000004;
                enemy.HP = Memory.ReadInt(currentAddress).ToString();
                currentAddress += 0x00000004;
                enemy.Family = Memory.ReadShort(currentAddress).ToString();
                currentAddress += Addresses.Instance.ShortOffset;                
                var absMultiplied = Memory.ReadShort(currentAddress) * Client.Options.ExpMultiplier;
                Memory.Write(currentAddress, (short)absMultiplied);
                enemy.ABS = Memory.ReadShort(currentAddress).ToString();
                currentAddress += Addresses.Instance.ShortOffset;
                var gildaMultiplied = Memory.ReadShort(currentAddress) * Client.Options.GoldMultiplier;
                Memory.Write(currentAddress, (short)gildaMultiplied);
                enemy.Gilda = Memory.ReadShort(currentAddress).ToString();
                currentAddress += Addresses.Instance.ShortOffset;
                enemy.Unknown2 = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 6));
                currentAddress += 0x00000006;
                enemy.Rage = Memory.ReadShort(currentAddress).ToString();
                currentAddress += Addresses.Instance.ShortOffset;
                enemy.Unknown3 = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 4));
                currentAddress += 0x00000004;
                enemy.Damage = Memory.ReadShort(currentAddress).ToString();
                currentAddress += Addresses.Instance.ShortOffset;
                enemy.Defense = Memory.ReadShort(currentAddress).ToString();
                currentAddress += Addresses.Instance.ShortOffset;
                enemy.BossFlag = Memory.ReadShort(currentAddress).ToString();
                currentAddress += Addresses.Instance.ShortOffset;
                enemy.Weaknesses = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 16));
                currentAddress += 0x00000010;
                enemy.Effectiveness = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 24));
                currentAddress += 0x00000018;
                enemy.Unknown4 = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 4));
                currentAddress += 0x00000004;
                enemy.IsRidepodEnemy = Memory.ReadByte(currentAddress).ToString();
                currentAddress += Addresses.Instance.ShortOffset;
                enemy.UnusedBits = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 2));
                currentAddress += Addresses.Instance.ShortOffset;
                enemy.Minions = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 4));
                currentAddress += 0x00000004;

                var itemSlot1 = Memory.ReadShort(currentAddress);
                currentAddress += Addresses.Instance.ShortOffset;
                var itemSlot2 = Memory.ReadShort(currentAddress);
                currentAddress += Addresses.Instance.ShortOffset;
                var itemSlot3 = Memory.ReadShort(currentAddress);
                currentAddress += Addresses.Instance.ShortOffset;

                enemy.Items = new List<ItemId>();
                if (itemSlot1 != 0x00)
                {
                    var id = ItemList.First(x => x.Id == itemSlot1);
                    enemy.Items.Add(id);
                }
                if (itemSlot2 != 0x00)
                {
                    var id = ItemList.First(x => x.Id == itemSlot2);
                    enemy.Items.Add(id);
                }
                if (itemSlot3 != 0x00)
                {
                    var id = ItemList.First(x => x.Id == itemSlot3);
                    enemy.Items.Add(id);
                }
                enemy.Unknown6 = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 10));
                currentAddress += 0x0000000A;
                var dungeon = Memory.ReadShort(currentAddress).ToString();
                enemy.Dungeon = DungeonList.First(x => x.id == int.Parse(dungeon)).Name;
                currentAddress += Addresses.Instance.ShortOffset;
                enemy.BestiarySpot = Memory.ReadShort(currentAddress).ToString();
                currentAddress += Addresses.Instance.ShortOffset;
                enemy.SharedHP = Memory.ReadShort(currentAddress).ToString();
                currentAddress += Addresses.Instance.ShortOffset;
                enemy.Unknown7 = Memory.ReadShort(currentAddress).ToString();
                currentAddress += Addresses.Instance.ShortOffset;
                enemies.Add(enemy);

                if (debug) Console.WriteLine($"Discovered enemy: {JsonConvert.SerializeObject(enemy, Formatting.Indented)}");


                currentAddress += 0x00000004;
            }

            if (debug) Console.WriteLine($"Found {enemies.Count} enemies");
            return enemies;
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
                    Floor floor = ReadFloor(currentAddress);
                    currentAddress += 0x0000014;
                    dungeon.Floors.Add(floor);
                    if (debug) Console.WriteLine(JsonConvert.SerializeObject(floor, Formatting.Indented));
                }
                currentAddress += 0x0000014;
            }
            return dungeons;
        }
        public static Floor ReadFloor(int currentAddress, bool debug = false)
        {
            if (debug) Console.WriteLine($"Starting floor read at {currentAddress.ToString("X8")}");
            Floor floor = new Floor();
            var data = new BitArray(Memory.ReadByteArray(currentAddress, 2));
            data[0] = true;
            byte[] newBytes = new byte[2];
            data.CopyTo(newBytes, 0);
            Memory.WriteByteArray(currentAddress, newBytes);
            currentAddress += Addresses.Instance.ShortOffset;
            if (debug) Console.WriteLine($"Reading {currentAddress.ToString("X8")}");
            var monstersKilled = Memory.ReadShort(currentAddress);
            currentAddress += Addresses.Instance.ShortOffset;
            if (debug) Console.WriteLine($"Reading {currentAddress.ToString("X8")}");
            var timesVisited = Memory.ReadShort(currentAddress);

            floor.IsUnlocked = data[0].ToString();
            floor.IsFinished = data[1].ToString();
            var unknown1 = data[2].ToString();
            floor.SpecialMedalCompleted = data[3].ToString();

            floor.ClearMedalCompleted = data[4].ToString();
            floor.FishMedalCompleted = data[5].ToString();
            var unknown3 = data[6].ToString();
            floor.SphedaMedalCompleted = data[7].ToString();

            floor.GotGeostone = data[8].ToString();
            floor.DownloadedGeostone = data[9].ToString();
            floor.KilledAllMonsters = data[10].ToString();

            floor.MonstersKilled = monstersKilled;
            floor.TimesVisited = timesVisited;

            return floor;
        }
    }
}