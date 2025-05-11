using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Avalonia.Animation;
using DC2AP.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP
{
    public static class Helpers
    {


        public static List<DarkCloud2Item> GetItemIds()
        {
            var json = OpenEmbeddedResource("DC2AP.Resources.ItemIds.json");
            var list = JsonConvert.DeserializeObject<List<DarkCloud2Item>>(json);
            return list;
        }
        public static List<QuestId> GetQuestIds()
        {
            var json = OpenEmbeddedResource("DC2AP.Resources.QuestIds.json");
            var list = JsonConvert.DeserializeObject<List<QuestId>>(json);
            return list;
        }
        public static List<Dungeon> GetDungeons()
        {
            var json = OpenEmbeddedResource("DC2AP.Resources.Dungeons.json");
            var list = JsonConvert.DeserializeObject<List<Dungeon>>(json);
            return list;
        }
        public static List<Location> GetLocations()
        {
            var json = OpenEmbeddedResource("DC2AP.Resources.Locations.json");
            var list = JsonConvert.DeserializeObject<List<Location>>(json);
            return list;
        }
        public static string OpenEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonFile = reader.ReadToEnd();
                return jsonFile;
            }
        }
        public static int ToAPId(int gameId)
        {
            return gameId + 694200000;
        }
        public static int ToGameId(int apId)
        {
            return apId - 694200000;
        }
        private static bool GetBitValue(byte value, int bitIndex)
        {
            return (value & (1 << bitIndex)) != 0;
        }
        public static void AddItem(Item item, PlayerState playerState, bool IsArchipelago = true)
        {
            playerState.IsReceivingArchipelagoItem = IsArchipelago;
            var alreadyHave = playerState.Inventory.Any(x => x.Id == item.Id);
            if (alreadyHave)
            {
                var slotNum = playerState.GetFirstSlot((int)item.Id);
                var address = GetItemSlotAddress(slotNum);
                var currentQuantity = Memory.ReadShort(address + 0x0000000E);
                item.Quantity += currentQuantity;
                WriteItem(item, address);
            }
            else
            {
                var slotNum = playerState.GetFirstSlot(0);
                var address = GetItemSlotAddress(slotNum);
                WriteItem(item, address);

            }
        }
        public static void RemoveItem(Item item, PlayerState playerState)
        {
            var slot = playerState.GetFirstSlot((int)item.Id);
            if (slot == -1) return; //Player does not have that item
            var address = GetItemSlotAddress(slot);
            var currentQuantity = Memory.ReadShort(address + 0x0000000E);
            item.Quantity = currentQuantity - 1;
            if (item.Quantity == 0)
            {
                RemoveAllItem(item, playerState);
            }
            else
            {
                WriteItem(item, address);
            }
        }
        public static void RemoveAllItem(Item item, PlayerState playerState)
        {
            var slot = playerState.GetFirstSlot((int)item.Id);
            if (slot == -1) return; //Player does not have that item
            var address = GetItemSlotAddress(slot);

            WriteItem(new Item() { Id = 0, Quantity = 0, IsProgression = false, Name = "null" }, address);
        }
        public static void WriteItem(Item item, ulong address)
        {
            ReadItem(address);
            Memory.Write(address, (ushort)item.Id);
            Memory.Write(address + 0x0000000F, (ushort)item.Quantity);
        }
        public static void ReadItem(ulong address)
        {
            for (int i = 0; i < 54; i++)
            {
                var value = Memory.ReadShort(address + (ulong)(2 * i));
            }
        }
        public static ulong GetItemSlotAddress(int slotNum)
        {
            var startAddress = Addresses.InventoryStartAddress;
            ulong offset = (uint)(0x0000006c * (slotNum));
            ulong slotAddress = startAddress + offset;
            return slotAddress;
        }
        public static long GetLocationFromProgressionItem(int progressionId)
        {
            var itemList = GetItemIds();
            var current = itemList.FirstOrDefault(x => x.Id == progressionId);
            if (current == null) return -1;
            return current.locationId;

        }
        static Chest ReadChest(ulong startAddress, bool isDouble = false)
        {
            Chest chest = new Chest() { IsDoubleChest = isDouble };
            var currentAddress = startAddress + Addresses.IntOffset;
            chest.Item1 = Memory.ReadShort(currentAddress);
            currentAddress += Addresses.ShortOffset;
            if (isDouble) chest.Item2 = Memory.ReadShort(currentAddress);
            currentAddress += Addresses.ShortOffset;
            chest.Quantity1 = Memory.ReadShort(currentAddress);
            currentAddress += Addresses.ShortOffset;
            if (isDouble) chest.Quantity2 = Memory.ReadShort(currentAddress);
            return chest;
        }
        static void AddChestItem(ulong startAddress, int id, int quantity)
        {
            startAddress += Addresses.IntOffset;
            Log.Logger.Information($"Setting Chest contents to {id}");
            Memory.Write(startAddress, BitConverter.GetBytes(id));
            startAddress += Addresses.IntOffset;
            Memory.Write(startAddress, BitConverter.GetBytes(quantity));
            Log.Logger.Information("Added item!");
        }
        static void AddDoubleChestItems(ulong startAddress, int id1, int quantity1, int id2, int quantity2)
        {
            startAddress += Addresses.IntOffset;
            var currentItem = Memory.ReadByte(startAddress);
            Log.Logger.Information($"replacing {currentItem} with {id1}");
            Memory.Write(startAddress, BitConverter.GetBytes(id1));
            startAddress += Addresses.ShortOffset;
            var currentItem2 = Memory.ReadByte(startAddress);
            Log.Logger.Information($"replacing {currentItem2} with {id2}");
            Memory.Write(startAddress, BitConverter.GetBytes(id2));
            startAddress += Addresses.ShortOffset;
            Memory.Write(startAddress, BitConverter.GetBytes(quantity1));
            startAddress += Addresses.ShortOffset;
            Memory.Write(startAddress, BitConverter.GetBytes(quantity2));
        }
        public static List<Chest> ReadChests()
        {
            List<Chest> chests = new List<Chest>();
            var chestStartAddress = Addresses.DungeonAreaChestAddress[Memory.ReadByte(Addresses.CurrentDungeon)];
            var currentChestAddress = chestStartAddress;
            var chestId = 0;
            while (chestId != 1)
            {
                chestId = Memory.ReadByte(currentChestAddress);
                var chest = ReadChest(currentChestAddress, chestId >= 128);
                chests.Add(chest);
                currentChestAddress += 0x00000070;
            }
            Log.Logger.Information("End of chests");
            return chests;
        }
        public static Floor ReadFloor(ulong currentAddress, bool debug = false)
        {
            if (debug) Log.Logger.Information($"Starting floor read at {currentAddress.ToString("X8")}");
            Floor floor = new Floor();
            var data = new BitArray(Memory.ReadByteArray(currentAddress, 2));
            data[0] = true;
            byte[] newBytes = new byte[2];
            data.CopyTo(newBytes, 0);
            Memory.WriteByteArray(currentAddress, newBytes);
            currentAddress += Addresses.ShortOffset;
            if (debug) Log.Logger.Information($"Reading {currentAddress.ToString("X8")}");
            var monstersKilled = Memory.ReadShort(currentAddress);
            currentAddress += Addresses.ShortOffset;
            if (debug) Log.Logger.Information($"Reading {currentAddress.ToString("X8")}");
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
        public static List<Enemy> ReadEnemies()
        {
            var itemList = GetItemIds();
            var dungeonList = GetDungeons();
            List<Enemy> enemies = new List<Enemy>();
            var currentAddress = Addresses.EnemyStartAddress + Addresses.IntOffset;
            for (int i = 0; i < 280; i++)
            {
                var enemy = Memory.ReadObject<Enemy>(currentAddress);
                enemies.Add(enemy);
                currentAddress += 0xB8;
            }
            return enemies;
        }
        public static void ShuffleEnemies(List<Enemy> enemies)
        {
            var newOrder = enemies.ToArray();
            new Random().Shuffle(newOrder);
            var currentAddress = Addresses.EnemyStartAddress + Addresses.IntOffset;
            foreach (var enemy in newOrder)
            {
                Memory.WriteObject(currentAddress, enemy);
                currentAddress += 0xB8;
            }
        }
        //public static List<Enemy> ReadEnemies(bool debug = false)
        //{
        //    var itemList = GetItemIds();
        //    var dungeonList = GetDungeons();
        //    List<Enemy> enemies = new List<Enemy>();
        //    var currentAddress = Addresses.EnemyStartAddress;
        //    currentAddress += Addresses.IntOffset;
        //    for (int i = 0; i < 280; i++)
        //    {
        //        Enemy enemy = new Enemy();
        //        enemy.Name = Memory.ReadString(currentAddress, 32);
        //        currentAddress += 0x00000020;
        //        enemy.ModelAI = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 32));
        //        currentAddress += 0x00000020;
        //        var modelType = Memory.ReadInt(currentAddress).ToString();
        //        enemy.ModelType = Helpers.GetModelType(modelType);
        //        currentAddress += Addresses.IntOffset;
        //        enemy.Sound = Memory.ReadInt(currentAddress).ToString();
        //        currentAddress += Addresses.IntOffset;
        //        enemy.Unknown1 = Memory.ReadInt(currentAddress).ToString();
        //        currentAddress += Addresses.IntOffset;
        //        enemy.HP = Memory.ReadInt(currentAddress).ToString();
        //        currentAddress += Addresses.IntOffset;
        //        enemy.Family = Memory.ReadShort(currentAddress).ToString();
        //        currentAddress += Addresses.ShortOffset;
        //        //    var absMultiplied = Memory.ReadShort(currentAddress) *  Client.Options.ExpMultiplier;
        //        //    Memory.Write(currentAddress, (short)absMultiplied);
        //        enemy.ABS = Memory.ReadShort(currentAddress).ToString();
        //        currentAddress += Addresses.ShortOffset;
        //        //    var gildaMultiplied = Memory.ReadShort(currentAddress) * Client.Options.GoldMultiplier;
        //        //    Memory.Write(currentAddress, (short)gildaMultiplied);
        //        enemy.Gilda = Memory.ReadShort(currentAddress).ToString();
        //        currentAddress += Addresses.ShortOffset;
        //        enemy.Unknown2 = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 6));
        //        currentAddress += 0x00000006;
        //        enemy.Rage = Memory.ReadShort(currentAddress).ToString();
        //        currentAddress += Addresses.ShortOffset;
        //        enemy.Unknown3 = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 4));
        //        currentAddress += Addresses.IntOffset;
        //        enemy.Damage = Memory.ReadShort(currentAddress).ToString();
        //        currentAddress += Addresses.ShortOffset;
        //        enemy.Defense = Memory.ReadShort(currentAddress).ToString();
        //        currentAddress += Addresses.ShortOffset;
        //        enemy.BossFlag = Memory.ReadShort(currentAddress).ToString();
        //        currentAddress += Addresses.ShortOffset;
        //        enemy.Weaknesses = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 16));
        //        currentAddress += 0x00000010;
        //        enemy.Effectiveness = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 24));
        //        currentAddress += 0x00000018;
        //        enemy.Unknown4 = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 4));
        //        currentAddress += Addresses.IntOffset;
        //        enemy.IsRidepodEnemy = Memory.ReadByte(currentAddress).ToString();
        //        currentAddress += Addresses.ShortOffset;
        //        enemy.UnusedBits = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 2));
        //        currentAddress += Addresses.ShortOffset;
        //        enemy.Minions = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 4));
        //        currentAddress += Addresses.IntOffset;

        //        var itemSlot1 = Memory.ReadShort(currentAddress);
        //        currentAddress += Addresses.ShortOffset;
        //        var itemSlot2 = Memory.ReadShort(currentAddress);
        //        currentAddress += Addresses.ShortOffset;
        //        var itemSlot3 = Memory.ReadShort(currentAddress);
        //        currentAddress += Addresses.ShortOffset;

        //        enemy.Items = new List<DarkCloud2Item>();
        //        if (itemSlot1 != 0x00)
        //        {
        //            var id = itemList.First(x => x.Id == itemSlot1);
        //            enemy.Items.Add(id);
        //        }
        //        if (itemSlot2 != 0x00)
        //        {
        //            var id = itemList.First(x => x.Id == itemSlot2);
        //            enemy.Items.Add(id);
        //        }
        //        if (itemSlot3 != 0x00)
        //        {
        //            var id = itemList.First(x => x.Id == itemSlot3);
        //            enemy.Items.Add(id);
        //        }
        //        enemy.Unknown6 = BitConverter.ToString(Memory.ReadByteArray(currentAddress, 10));
        //        currentAddress += 0x0000000A;
        //        var dungeon = Memory.ReadShort(currentAddress).ToString();
        //        enemy.Dungeon = dungeonList.First(x => x.id == int.Parse(dungeon)).Name;
        //        currentAddress += Addresses.ShortOffset;
        //        enemy.BestiarySpot = Memory.ReadShort(currentAddress).ToString();
        //        currentAddress += Addresses.ShortOffset;
        //        enemy.SharedHP = Memory.ReadShort(currentAddress).ToString();
        //        currentAddress += Addresses.ShortOffset;
        //        enemy.Unknown7 = Memory.ReadShort(currentAddress).ToString();
        //        currentAddress += Addresses.ShortOffset;
        //        enemies.Add(enemy);

        //        if (debug) Log.Logger.Information($"Discovered enemy: {JsonConvert.SerializeObject(enemy, Formatting.Indented)}");


        //        currentAddress += 0x00000004;
        //    }

        //    if (debug) Log.Logger.Information($"Found {enemies.Count} enemies");
        //    return enemies;
        //}
        public static string GetHabitat(string id)
        {
            switch (id)
            {
                case ("0"):
                    return "Underground water channel";
                case ("1"):
                    return "Rainbow butterfly wood";
                case ("2"):
                    return "Starlight Canyon";
                case ("3"):
                    return "Oceans roar cave";
                case ("4"):
                    return "Mount Gundor";
                case ("5"):
                    return "Moon flower palace";
                case ("6"):
                    return "Zelmite Mine";
            }
            return "";
        }

        public static string GetModelType(int id)
        {
            switch (id)
            {
                case (0):
                    return "Rat";
                case (1):
                    return "Vanguard";
                case (2):
                    return "Frog";
                case (3):
                    return "Tree";
                case (4):
                    return "Small Tree";
                case (5):
                    return "Fox";
                case (6):
                    return "Balloon";
                case (7):
                    return "Tortoise";
                case (8):
                    return "Turtle";
                case (9):
                    return "Clown";
                case (10):
                    return "Griffon";
                case (11):
                    return "Pixie";
                case (12):
                    return "Elephant";
                case (13):
                    return "Flower1";
                case (14):
                    return "Beast";
                case (15):
                    return "Ghost";
                case (16):
                    return "Dragon";
                case (17):
                    return "Tapir";
                case (18):
                    return "Spider";
                case (19):
                    return "Fire Elemental";
                case (20):
                    return "Ice Elemental";
                case (21):
                    return "Lightning Elemental";
                case (22):
                    return "Water Elemental";
                case (23):
                    return "Wind Elemental";
                case (24):
                    return "Mask1";
                case (25):
                    return "Pumpkin";
                case (26):
                    return "Mummy";
                case (27):
                    return "Flower2";
                case (28):
                    return "Fire Gemron";
                case (29):
                    return "Ice Gemron";
                case (30):
                    return "Lightning Gemron";
                case (31):
                    return "Wind Gemron";
                case (32):
                    return "Holy Gemron";
                case (33):
                    return "Mask2";
                case (34):
                    return "Ram";
                case (35):
                    return "Mole";
                case (36):
                    return "Snake";
                case (37):
                    return "Fish";
                case (38):
                    return "Naga";
                case (39):
                    return "Dog Statue";
                case (40):
                    return "Rock";
                case (41):
                    return "Statue";
                case (42):
                    return "Golem";
                case (43):
                    return "Big Skeleton";
                case (44):
                    return "Skeleton";
                case (45):
                    return "Pirate Skeleton";
                case (46):
                    return "Pirate Captain Skeleton";
                case (47):
                    return "Dagger Skeleton";
                case (48):
                    return "Face";
                case (49):
                    return "Fairy";
                case (50):
                    return "Moon";
                case (51):
                    return "Shadow";
                case (52):
                    return "Priest";
                case (53):
                    return "Knight";
                case (54):
                    return "Tank";
                case (55):
                    return "Bomb";
                case (56):
                    return "Card (Clubs)";
                case (57):
                    return "Card (Hearts)";
                case (58):
                    return "Card (Spades)";
                case (59):
                    return "Card (Diamonds)";
                case (60):
                    return "Card (Joker)";
                case (61):
                    return "Mimic";
                case (62):
                    return "King Mimic";
                case (63):
                    return "Sonic Bomber";
                case (64):
                    return "Barrel Robot";
                case (150):
                    return "Bat";
            }
            return "";
        }

        internal static int GetLocationFromProgressionItem(long itemId)
        {
            var locations = GetLocations();
            Dictionary<string, int> locationToItemDict = new Dictionary<string, int>()
            {
                { "Grape Juice", 370 },
                { "Lafrescia Seed", 361 },
                { "Fishing Rod", 302 },
                { "Earth Gem", 365 },
                { "Starglass", 371 },
                { "Miracle Dumplings", 364 },
                { "White Windflower", 363 },
                { "Wind Gem", 367 },
                { "Electrim Worm", 370 },
                { "Shell Talkie", 373 },
                { "Secret Dragon Remedy", 375 },
                { "Water Gem", 366 },
                { "Time Bomb", 372 },
                { "Fire Horn", 354 },
                { "Fire Gem", 368 },
                { "Flower of the Sun", 374 },

            };
            if (locationToItemDict.Any(x => x.Value == itemId))
            {
                var locationName = locationToItemDict.First(x => x.Value == itemId).Key;
                var location = locations.First(x => x.Name.ToLower().Contains(locationName.ToLower()));
                if (location != null) return location.Id;
            }
            return -1;
        }
    }
}
