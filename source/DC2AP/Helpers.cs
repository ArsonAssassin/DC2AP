using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.Enums;
using DC2AP.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DC2AP
{
    public static class Helpers
    {

        public static T Random<T>(this IEnumerable<T> list) where T : struct
        {
            return list.ToList()[new Random().Next(0, list.Count())];
        }
        public static List<ItemId> GetItemIds()
        {
            var json = OpenEmbeddedResource("DC2AP.Resources.ItemIds.json");
            var list = JsonConvert.DeserializeObject<List<ItemId>>(json);
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
                var slotNum = playerState.GetFirstSlot(item.Id);
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
            var slot = playerState.GetFirstSlot(item.Id);
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
            var slot = playerState.GetFirstSlot(item.Id);
            if (slot == -1) return; //Player does not have that item
            var address = GetItemSlotAddress(slot);

            WriteItem(new Item() { Id = 0, Quantity = 0, IsProgression = false, Name = "null" }, address);
        }
        public static void WriteItem(Item item, uint address)
        {
            Memory.Write(address, (ushort)item.Id);
            Memory.Write(address + 0x0000000E, (ushort)item.Quantity);
        }
        public static uint GetItemSlotAddress(int slotNum)
        {
            var startAddress = Addresses.Instance.InventoryStartAddress;
            uint offset = (uint)(0x0000006c * (slotNum));
            uint slotAddress = startAddress + offset;
            return slotAddress;
        }
        public static long GetLocationFromProgressionItem(int progressionId)
        {
            var itemList = GetItemIds();
            var current = itemList.FirstOrDefault(x => x.Id == progressionId);
            if (current == null) return -1;
            return current.locationId;

        }
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

        public static string GetModelType(string id)
        {
            switch (id)
            {
                case ("0"):
                    return "Rat";
                case ("1"):
                    return "Vanguard";
                case ("2"):
                    return "Frog";
                case ("3"):
                    return "Tree";
                case ("4"):
                    return "Small Tree";
                case ("5"):
                    return "Fox";
                case ("6"):
                    return "Balloon";
                case ("7"):
                    return "Tortoise";
                case ("8"):
                    return "Turtle";
                case ("9"):
                    return "Clown";
                case ("10"):
                    return "Griffon";
                case ("11"):
                    return "Pixie";
                case ("12"):
                    return "Elephant";
                case ("13"):
                    return "Flower1";
                case ("14"):
                    return "Beast";
                case ("15"):
                    return "Ghost";
                case ("16"):
                    return "Dragon";
                case ("17"):
                    return "Tapir";
                case ("18"):
                    return "Spider";
                case ("19"):
                    return "Fire Elemental";
                case ("20"):
                    return "Ice Elemental";
                case ("21"):
                    return "Lightning Elemental";
                case ("22"):
                    return "Water Elemental";
                case ("23"):
                    return "Wind Elemental";
                case ("24"):
                    return "Mask1";
                case ("25"):
                    return "Pumpkin";
                case ("26"):
                    return "Mummy";
                case ("27"):
                    return "Flower2";
                case ("28"):
                    return "Fire Gemron";
                case ("29"):
                    return "Ice Gemron";
                case ("30"):
                    return "Lightning Gemron";
                case ("31"):
                    return "Wind Gemron";
                case ("32"):
                    return "Holy Gemron";
                case ("33"):
                    return "Mask2";
                case ("34"):
                    return "Ram";
                case ("35"):
                    return "Mole";
                case ("36"):
                    return "Snake";
                case ("37"):
                    return "Fish";
                case ("38"):
                    return "Naga";
                case ("39"):
                    return "Dog Statue";
                case ("40"):
                    return "Rock";
                case ("41"):
                    return "Statue";
                case ("42"):
                    return "Golem";
                case ("43"):
                    return "Big Skeleton";
                case ("44"):
                    return "Skeleton";
                case ("45"):
                    return "Pirate Skeleton";
                case ("46"):
                    return "Pirate Captain Skeleton";
                case ("47"):
                    return "Dagger Skeleton";
                case ("48"):
                    return "Face";
                case ("49"):
                    return "Fairy";
                case ("50"):
                    return "Moon";
                case ("51"):
                    return "Shadow";
                case ("52"):
                    return "Priest";
                case ("53"):
                    return "Knight";
                case ("54"):
                    return "Tank";
                case ("55"):
                    return "Bomb";
                case ("56"):
                    return "Card (Clubs)";
                case ("57"):
                    return "Card (Hearts)";
                case ("58"):
                    return "Card (Spades)";
                case ("59"):
                    return "Card (Diamonds)";
                case ("60"):
                    return "Card (Joker)";
                case ("61"):
                    return "Mimic";
                case ("62"):
                    return "King Mimic";
                case ("63"):
                    return "Sonic Bomber";
                case ("64"):
                    return "Barrel Robot";
                case ("150"):
                    return "Bat";
            }
            return "";
        }
    }
}
