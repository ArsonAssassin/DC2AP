using Newtonsoft.Json;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace DC2AP
{
    static class Program
    {
        public static string GameVersion { get; set; }
        public static List<ItemId> ItemList { get; set; }
        public static List<Enemy> EnemyList { get; set; }
        static void Main()
        {
            Console.SetBufferSize(Console.BufferWidth, 32766);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.WriteLine("DC2AP - Dark Cloud 2 Archipelago Randomizer");


            Console.WriteLine("Connecting to PCSX2");
            var pid = Memory.PCSX2_PROCESSID;
            if (pid == 0)
            {
                Console.WriteLine("PCSX2 not found.");
                Console.WriteLine("Press any key to exit.");
                Console.Read();
                System.Environment.Exit(0);
            }
            GameVersion = Memory.ReadInt(0x203694D0) == 1701667175 ? "PAL" : Memory.ReadInt(0x20364BD0) == 1701667175 ? "US" : "";
            if (string.IsNullOrWhiteSpace(GameVersion))
            {
                Console.WriteLine("Dark cloud 2 is not loaded, please load the game and try again.");
                Console.WriteLine("Press any key to exit.");
                Console.Read();
                System.Environment.Exit(0);
            }
            Console.WriteLine($"Connected to Dark Cloud 2 ({GameVersion})");

            Console.WriteLine("Building Item List");
            ItemList = Helpers.GetItemIds();
            Console.WriteLine("Building Enemy List");
            EnemyList = ReadEnemies();

            Console.WriteLine("Beginning main loop.");

            while (true)
            {
                if (Memory.ReadByte(Addresses.Instance.CurrentFloor) > 0)
                {
                    if (Addresses.Instance.CurrentFloor != Addresses.Instance.PreviousFloor)
                    {
                        Console.WriteLine("Moved to new floor");
                        Thread.Sleep(6000);

                        TestCode(5);

                        var currentAddress = Addresses.Instance.DungeonAreaChestAddress[Memory.ReadByte(Addresses.Instance.CurrentDungeon)] + 0x0000005C;
                        while (Memory.ReadShort(currentAddress) != 306)
                        {
                            Thread.Sleep(1);
                            if (Memory.ReadByte(Addresses.Instance.CurrentFloor) == 0 || Memory.ReadByte(Addresses.Instance.DungeonCheckAddress) > 2)
                            {
                                Console.WriteLine("Exited dungeon");
                                break;
                            }
                        }
                        Thread.Sleep(1000);
                        Console.WriteLine("Map spawned on first chest");
                        Memory.Write(currentAddress, (ushort)305);
                        currentAddress += 0x00000070;
                        Console.WriteLine("Magic crystal spawned on second chest");
                        Memory.Write(currentAddress, (ushort)306);
                        currentAddress += 0x0000006C;
                        for (int i = 0; i < 25; i++)
                        {
                            var chest = Memory.ReadByte(currentAddress);
                            if (chest == 1)
                            {
                                Console.WriteLine("End of chests");
                                break;
                            }
                            if (chest < 128)
                            {
                                Console.WriteLine("Found single chest");
                                AddChestItem(currentAddress, 268, 1);
                                currentAddress += 0x00000004;
                                currentAddress += 0x00000004;
                                currentAddress += 0x00000068;
                            }
                            else
                            {
                                Console.WriteLine("Found double chest");
                                AddDoubleChestItems(currentAddress, 268, 1, 268, 1);
                                currentAddress += 0x00000004;
                                currentAddress += 0x00000002;
                                currentAddress += 0x00000002;
                                currentAddress += 0x00000002;
                                currentAddress += 0x00000066;
                            }
                        }


                        Addresses.Instance.PreviousFloor = Addresses.Instance.CurrentFloor;
                    }
                }
                else
                {
                    Addresses.Instance.PreviousFloor = 200;
                }

                //Handle exiting the game
                if (GameVersion == "PAL")
                {
                    if (Memory.ReadInt(0x203694D0) != 1701667175)
                    {
                        Thread.Sleep(1000);
                        if (Memory.ReadInt(0x203694D0) != 1701667175)
                        {
                            System.Environment.Exit(0);
                        }
                    }
                }
                else if (GameVersion == "US")
                {
                    if (Memory.ReadInt(0x20364BD0) != 1701667175)
                    {
                        Thread.Sleep(1000);
                        if (Memory.ReadInt(0x20364BD0) != 1701667175)
                        {
                            System.Environment.Exit(0);
                        }
                    }
                }

                Thread.Sleep(1);
            }
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
            startAddress += 0x00000002;
            var currentItem2 = Memory.ReadByte(startAddress);
            Console.WriteLine($"replacing {currentItem2} with {id2}");
            Memory.Write(startAddress, BitConverter.GetBytes(id2));
            startAddress += 0x00000002;
            Memory.Write(startAddress, BitConverter.GetBytes(quantity1));
            startAddress += 0x00000002;
            Memory.Write(startAddress, BitConverter.GetBytes(quantity2));
        }

        static void TestCode(int id1)
        {
            var currentGil = Memory.ReadInt(Addresses.Instance.PlayerGilda);
            Console.WriteLine($"Current Gilda: {currentGil}");
            var currentMedals = Memory.ReadShort(Addresses.Instance.PlayerMedals);
            Console.WriteLine($"current medals: {currentMedals}");

            var inventory = ReadInventory();
            var dungeonFloors = ReadFloorData();
        }

        static List<Enemy> ReadEnemies(bool debug = false)
        {
            List<Enemy> enemies = new List<Enemy>();
            var currentAddress = 0x2033D9E0;
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
                currentAddress += 0x00000002;
                enemy.ABS = Memory.ReadShort(currentAddress).ToString();
                currentAddress += 0x00000002;
                enemy.Gilda = Memory.ReadShort(currentAddress).ToString();
                currentAddress += 0x00000002;
                enemy.Unknown2 = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 6));
                currentAddress += 0x00000006;
                enemy.Rage = Memory.ReadShort(currentAddress).ToString();
                currentAddress += 0x00000002;
                enemy.Unknown3 = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 4));
                currentAddress += 0x00000004;
                enemy.Damage = Memory.ReadShort(currentAddress).ToString();
                currentAddress += 0x00000002;
                enemy.Defense = Memory.ReadShort(currentAddress).ToString();
                currentAddress += 0x00000002;
                enemy.BossFlag = Memory.ReadShort(currentAddress).ToString();
                currentAddress += 0x00000002;
                enemy.Weaknesses = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 16));
                currentAddress += 0x00000010;
                enemy.Effectiveness = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 24));
                currentAddress += 0x00000018;
                enemy.Unknown4 = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 4));
                currentAddress += 0x00000004;
                enemy.IsRidepodEnemy = Memory.ReadByte(currentAddress).ToString();
                currentAddress += 0x00000002;
                enemy.UnusedBits = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 2));
                currentAddress += 0x00000002;
                enemy.Minions = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 4));
                currentAddress += 0x00000004;

                var itemSlot1 = Memory.ReadShort(currentAddress);
                currentAddress += 0x00000002;
                var itemSlot2 = Memory.ReadShort(currentAddress);
                currentAddress += 0x00000002;
                var itemSlot3 = Memory.ReadShort(currentAddress);
                currentAddress += 0x00000002;

                enemy.Items = new List<ItemId>();
                if(itemSlot1 != 0x00)
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
                enemy.Dungeon = Helpers.GetHabitat(dungeon);
                currentAddress += 0x00000002;
                enemy.BestiarySpot = Memory.ReadShort(currentAddress).ToString();
                currentAddress += 0x00000002;
                enemy.SharedHP = Memory.ReadShort(currentAddress).ToString();
                currentAddress += 0x00000002;
                enemy.Unknown7 = Memory.ReadShort(currentAddress).ToString();
                currentAddress += 0x00000002;
                enemies.Add(enemy);

                if(debug) Console.WriteLine($"Discovered enemy: {JsonConvert.SerializeObject(enemy, Formatting.Indented)}");


                currentAddress += 0x00000004;
            }

            if (debug) Console.WriteLine($"Found {enemies.Count} enemies");
            return enemies;
        }
        static List<Item> ReadInventory(bool debug = false)
        {
            List<Item> inventory = new List<Item>();

            // start of inventory
            var startAddress = 0x21E1EAB2;


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

        static List<Floor> ReadFloorData(bool debug = false)
        {
            List<Floor> floors = new List<Floor>();
            var currentAddress = 0x21E1DE22;

                if(debug) Console.WriteLine("Discovering Sewer Dungeon floors");
            for (int i = 0; i < 8; i++)
            {
                Floor floor = ReadFloor(currentAddress);
                currentAddress += 0x0000014;
                floors.Add(floor);
                if (debug) Console.WriteLine(JsonConvert.SerializeObject(floor, Formatting.Indented));
            }

            currentAddress += 0x0000014;

            if (debug) Console.WriteLine("Discovering Rainbow woods Dungeon floors");
            for (int i = 0; i < 15; i++)
            {
                Floor floor = ReadFloor(currentAddress);
                currentAddress += 0x0000014;

                floors.Add(floor);
                if (debug) Console.WriteLine(JsonConvert.SerializeObject(floor, Formatting.Indented));
            }

            currentAddress += 0x0000014;
            if (debug) Console.WriteLine("Discovering Starlight Dungeon floors");
            for (int i = 0; i < 23; i++)
            {
                Floor floor = ReadFloor(currentAddress);
                currentAddress += 0x0000014;

                floors.Add(floor);
                if (debug) Console.WriteLine(JsonConvert.SerializeObject(floor, Formatting.Indented));
            }

            currentAddress += 0x0000014;
            if (debug) Console.WriteLine("Discovering Ocean cavern Dungeon floors");
            for (int i = 0; i < 19; i++)
            {
                Floor floor = ReadFloor(currentAddress);
                currentAddress += 0x0000014;

                floors.Add(floor);
                if (debug) Console.WriteLine(JsonConvert.SerializeObject(floor, Formatting.Indented));
            }

            currentAddress += 0x0000014;
            if (debug) Console.WriteLine("Discovering Mount Gundor Dungeon floors");
            for (int i = 0; i < 21; i++)
            {
                Floor floor = ReadFloor(currentAddress);
                currentAddress += 0x0000014;

                floors.Add(floor);
                if (debug) Console.WriteLine(JsonConvert.SerializeObject(floor, Formatting.Indented));
            }

            currentAddress += 0x0000014;
            if (debug) Console.WriteLine("Discovering Moon flower palace Dungeon floors");
            for (int i = 0; i < 27; i++)
            {
                Floor floor = ReadFloor(currentAddress);
                currentAddress += 0x0000014;

                floors.Add(floor);
                if (debug) Console.WriteLine(JsonConvert.SerializeObject(floor, Formatting.Indented));
            }

            currentAddress += 0x0000014;
            if (debug) Console.WriteLine("Discovering Zenite mine Dungeon floors");
            for (int i = 0; i < 38; i++)
            {
                Floor floor = ReadFloor(currentAddress);
                currentAddress += 0x0000014;

                floors.Add(floor);
                if (debug) Console.WriteLine(JsonConvert.SerializeObject(floor, Formatting.Indented));
            }
            return floors;
        }
        static Floor ReadFloor(int currentAddress, bool debug = false)
        {
            if (debug) Console.WriteLine($"Starting floor read at {currentAddress.ToString("X8")}");
            Floor floor = new Floor();
            var data = new BitArray(Memory.ReadByteArray(currentAddress, 2));
            currentAddress += 0x00000002;
            if (debug) Console.WriteLine($"Reading {currentAddress.ToString("X8")}");
            var monstersKilled = Memory.ReadShort(currentAddress);
            currentAddress += 0x00000002;
            if (debug) Console.WriteLine($"Reading {currentAddress.ToString("X8")}");
            var timesVisited = Memory.ReadShort(currentAddress);
            currentAddress += 0x00000002;

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