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

namespace DC2AP.Helpers
{
    public static class GeneralHelpers
    {
        public static List<DarkCloud2Item> DefaultItems {  get; set; }
        public static List<ItemId> ItemList { get; set; }
        public static List<Enemy> EnemyList { get; set; }
        public static List<QuestId> QuestList { get; set; }
        public static List<Dungeon> DungeonList { get; set; }
        public static void PopulateLists()
        {
            Log.Logger.Debug("Building Item List");
            ItemList = GetItemIds();
            Log.Logger.Debug("Building Default Item List");
            DefaultItems = GetDefaultItems();
            Log.Logger.Debug("Building Quest List");
            QuestList = GetQuestIds();
            Log.Logger.Debug("Building Dungeon List");
            DungeonList = PopulateDungeons();
            Log.Logger.Debug("Building Enemy List");
            EnemyList = ReadEnemies();
        }

        private static T DeserializeResource<T>(string resourceName)
        {
            var json = OpenEmbeddedResource(resourceName);
            return JsonConvert.DeserializeObject<T>(json)!;
        }
        public static List<ItemId> GetItemIds() =>
            DeserializeResource<List<ItemId>>("DC2AP.Resources.ItemIds.json");

        public static List<DarkCloud2Item> GetDefaultItems() =>
            DeserializeResource<List<DarkCloud2Item>>("DC2AP.Resources.DefaultItems.json");

        public static List<QuestId> GetQuestIds() =>
            DeserializeResource<List<QuestId>>("DC2AP.Resources.QuestIds.json");

        public static List<Dungeon> GetDungeons() =>
            DeserializeResource<List<Dungeon>>("DC2AP.Resources.Dungeons.json");
        public static List<ILocation> GetLocations()
        {
            var json = OpenEmbeddedResource("DC2AP.Resources.Locations.json");
            return Archipelago.Core.Json.LocationJsonHelper.Instance.DeserializeLocations(json);
        }
        public static string OpenEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();

        }
        internal static IEnumerable<(int ItemId, int Quantity)> GetRewardPack(long packId)
        {
            return packId switch
            {
                2000 => new[] { (268, 5), (294, 5), (298, 5), (352, 5), (381, 2) }, // Essential Pack
                2001 => new[] { (186, 1), (187, 1), (188, 1), (189, 1), (190, 1), (191, 1), (192, 1) }, // Gem Pack A
                2002 => new[] { (193, 1), (194, 1), (195, 1), (196, 1), (197, 1), (198, 1), (199, 1) }, // Gem Pack B
                2003 => new[] { (200, 1), (201, 1), (202, 1), (203, 1), (204, 1) }, // Coin Pack A
                2004 => new[] { (205, 1), (206, 1), (207, 1), (208, 1), (209, 1) }, // Coin Pack B
                2005 => new[] { (175, 2), (176, 2), (177, 2), (178, 2), (179, 2), (180, 2), (181, 2), (182, 2), (183, 1), (184, 2) }, // Crystal Pack
                2006 => new[] { (210, 50), (211, 50), (212, 50), (213, 50), (214, 50), (215, 50), (216, 50), (217, 50), (218, 50), (219, 50), (220, 50), (221, 50), (222, 50), (223, 50), (224, 50), (225, 50), (226, 50), (227, 50), (245, 50) }, // Georama Pack A
                2007 => new[] { (228, 25), (229, 25), (230, 25), (231, 25), (232, 25), (233, 25), (234, 25), (235, 25), (236, 25) }, // Georama Pack B
                _ => Array.Empty<(int, int)>()
            };
        }
        public static int ToAPId(int gameId) => gameId + Constants.AP_ID_OFFSET;
        public static int ToGameId(int apId) => apId - Constants.AP_ID_OFFSET;
        public static bool IsProgressionItem(int itemId)
        {
            var item = ItemList?.FirstOrDefault(x => x.Id == itemId);
            return item?.IsProgression ?? false;
        }
        private static bool GetBitValue(byte value, int bitIndex)
        {
            return (value & 1 << bitIndex) != 0;
        }
        public static void AddItem(DarkCloud2Item item, PlayerState playerState, int quantity = 1, bool IsArchipelago = true)
        {
            playerState.IsReceivingArchipelagoItem = IsArchipelago;
            var slotNum = playerState.GetFirstSlot(item.ItemId);
            if (slotNum == -1)
            {
                Log.Logger.Warning($"No available slot for item {item.Name}");
                return;
            }

            var address = GetItemSlotAddress(slotNum);
            var currentSlot = Memory.ReadStruct<InventorySlot>(address);

            InventorySlot slotData;
            if (currentSlot.ItemId == item.ItemId)
            {
                // Item already exists in this slot — update quantity only, preserve all other game data
                slotData = currentSlot;
                slotData.Quantity = (short)(currentSlot.Quantity + quantity);
            }
            else
            {
                // Empty slot — write fresh item data
                slotData = item.ToSlot();
                slotData.Quantity = (short)quantity;
            }
            WriteSlot(address, slotData);
            playerState.UpdateSlot(slotNum, slotData);
        }
        public static void RemoveItem(DarkCloud2Item item, PlayerState playerState)
        {
            playerState.IsReceivingArchipelagoItem = true;
            var slot = playerState.GetFirstSlot((int)item.Id);
            if (slot == -1) return;
            var address = GetItemSlotAddress(slot);
            var currentSlot = Memory.ReadStruct<InventorySlot>(address);

            if (currentSlot.Quantity <= 1)
            {
                RemoveAllItem(item, playerState);
            }
            else
            {
                currentSlot.Quantity = (short)(currentSlot.Quantity - 1);
                WriteSlot(address, currentSlot);
                playerState.UpdateSlot(slot, currentSlot);
            }
        }
        public static void RemoveAllItem(DarkCloud2Item item, PlayerState playerState)
        {
            playerState.IsReceivingArchipelagoItem = true;
            var slot = playerState.GetFirstSlot((int)item.Id);
            if (slot == -1) return;
            var address = GetItemSlotAddress(slot);
            WriteSlot(address, default);
            playerState.UpdateSlot(slot, default);
        }
        public static void WriteSlot(ulong address, InventorySlot slotData)
        {
            Memory.WriteStruct(address, slotData);
        }

        public static ulong GetItemSlotAddress(int slotNum)
        {
            var startAddress = Addresses.InventoryStartAddress;
            ulong offset = (uint)(Addresses.ItemSlotSize * slotNum);
            return startAddress + offset;
        }
        public static long GetLocationFromProgressionItem(int progressionId)
        {
            var current = ItemList?.FirstOrDefault(x => x.Id == progressionId);
            if (current == null || current.LocationId == 0) return -1;
            return current.LocationId;
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
            Log.Logger.Debug($"Setting Chest contents to {id}");
            Memory.Write(startAddress, BitConverter.GetBytes(id));
            startAddress += Addresses.IntOffset;
            Memory.Write(startAddress, BitConverter.GetBytes(quantity));
            Log.Logger.Debug("Added item!");
        }
        static void AddDoubleChestItems(ulong startAddress, int id1, int quantity1, int id2, int quantity2)
        {
            startAddress += Addresses.IntOffset;
            var currentItem = Memory.ReadByte(startAddress);
            Log.Logger.Debug($"replacing {currentItem} with {id1}");
            Memory.Write(startAddress, BitConverter.GetBytes(id1));
            startAddress += Addresses.ShortOffset;
            var currentItem2 = Memory.ReadByte(startAddress);
            Log.Logger.Debug($"replacing {currentItem2} with {id2}");
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
                currentChestAddress += 0x70;
            }
            Log.Logger.Information("End of chests");
            return chests;
        }
        public static Floor ReadFloor(ulong currentAddress, bool debug = false)
        {
            return Memory.ReadObject<Floor>(currentAddress);
        }
        public static List<Dungeon> PopulateDungeons()
        {
            List<Dungeon> dungeons = GetDungeons();

            var currentAddress = Addresses.DungeonStartAddress;

            foreach (var dungeon in dungeons)
            {
                dungeon.Floors = new List<Floor>();
                for (int i = 0; i < dungeon.FloorCount; i++)
                {
                    Floor floor = ReadFloor(currentAddress);
                    currentAddress += 0x14;
                    dungeon.Floors.Add(floor);
                    Log.Logger.Debug(JsonConvert.SerializeObject(floor, Formatting.Indented));
                }
                currentAddress += 0x14;
            }
            return dungeons;
        }
        public static List<Enemy> ReadEnemies()
        {
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
        public static void WriteEnemies(List<Enemy> enemies)
        {
            var currentAddress = Addresses.EnemyStartAddress + Addresses.IntOffset;
            foreach (var enemy in enemies)
            {
                Memory.WriteObject(currentAddress, enemy);
                currentAddress += 0xB8;
            }
        }
        public static List<Enemy> ShuffleEnemies(List<Enemy> enemies)
        {
            // Collect cosmetic properties (model, AI, name, sound) from all enemies
            var cosmetics = enemies.Select(e => new
            {
                e.Name,
                e.ModelAI,
                e.ModelType,
                e.Sound,
                e.Family,
                e.BestiaryId,
                e.DungeonId,
                e.IsRidepodEnemy,
                e.BestiarySpot,
                e.Minions
            }).ToArray();

            new Random().Shuffle(cosmetics);

            // Apply shuffled cosmetics while keeping all stats in place
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Name = cosmetics[i].Name;
                enemies[i].ModelAI = cosmetics[i].ModelAI;
                enemies[i].ModelType = cosmetics[i].ModelType;
                enemies[i].Sound = cosmetics[i].Sound;
                enemies[i].Family = cosmetics[i].Family;
                enemies[i].BestiaryId = cosmetics[i].BestiaryId;
                enemies[i].DungeonId = cosmetics[i].DungeonId;
                enemies[i].IsRidepodEnemy = cosmetics[i].IsRidepodEnemy;
                enemies[i].BestiarySpot = cosmetics[i].BestiarySpot;
                enemies[i].Minions = cosmetics[i].Minions;
            }

            return enemies;
        }
        public static string GetHabitat(string id) => id switch
        {
            "0" => "Underground water channel",
            "1" => "Rainbow butterfly wood",
            "2" => "Starlight Canyon",
            "3" => "Oceans roar cave",
            "4" => "Mount Gundor",
            "5" => "Moon flower palace",
            "6" => "Zelmite Mine",
            _ => string.Empty
        };

        public static string GetModelType(int id) => id switch
        {
            0 => "Rat",
            1 => "Vanguard",
            2 => "Frog",
            3 => "Tree",
            4 => "Small Tree",
            5 => "Fox",
            6 => "Balloon",
            7 => "Tortoise",
            8 => "Turtle",
            9 => "Clown",
            10 => "Griffon",
            11 => "Pixie",
            12 => "Elephant",
            13 => "Flower1",
            14 => "Beast",
            15 => "Ghost",
            16 => "Dragon",
            17 => "Tapir",
            18 => "Spider",
            19 => "Fire Elemental",
            20 => "Ice Elemental",
            21 => "Lightning Elemental",
            22 => "Water Elemental",
            23 => "Wind Elemental",
            24 => "Mask1",
            25 => "Pumpkin",
            26 => "Mummy",
            27 => "Flower2",
            28 => "Fire Gemron",
            29 => "Ice Gemron",
            30 => "Lightning Gemron",
            31 => "Wind Gemron",
            32 => "Holy Gemron",
            33 => "Mask2",
            34 => "Ram",
            35 => "Mole",
            36 => "Snake",
            37 => "Fish",
            38 => "Naga",
            39 => "Dog Statue",
            40 => "Rock",
            41 => "Statue",
            42 => "Golem",
            43 => "Big Skeleton",
            44 => "Skeleton",
            45 => "Pirate Skeleton",
            46 => "Pirate Captain Skeleton",
            47 => "Dagger Skeleton",
            48 => "Face",
            49 => "Fairy",
            50 => "Moon",
            51 => "Shadow",
            52 => "Priest",
            53 => "Knight",
            54 => "Tank",
            55 => "Bomb",
            56 => "Card (Clubs)",
            57 => "Card (Hearts)",
            58 => "Card (Spades)",
            59 => "Card (Diamonds)",
            60 => "Card (Joker)",
            61 => "Mimic",
            62 => "King Mimic",
            63 => "Sonic Bomber",
            64 => "Barrel Robot",
            150 => "Bat",
            _ => string.Empty
        };

    }
}
