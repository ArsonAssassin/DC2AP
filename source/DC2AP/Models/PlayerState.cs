using Archipelago.Core.Models;
using Archipelago.Core.Util;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static DC2AP.Models.Enums;

namespace DC2AP.Models
{
    public class PlayerState : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int Gilda { get; set; }
        public int MedalCount { get; set; }
        private int currentDungeon;
        private int currentFloor;

        public int CurrentDungeon
        {
            get => currentDungeon;
            set
            {
                if (currentDungeon != value)
                {
                    currentDungeon = value;
                    OnPropertyChanged();
                }
            }
        }
        public int CurrentFloor
        {
            get => currentFloor;
            set
            {
                if (currentFloor != value)
                {
                    currentFloor = value;
                    OnPropertyChanged();
                }
            }
        }
        private ObservableCollection<DarkCloud2Item> inventory;
        private List<DarkCloud2Item> oldInventory;
        public bool IsReceivingArchipelagoItem { get; set; }
        public ObservableCollection<DarkCloud2Item> Inventory
        {
            get => inventory;
            set
            {
                if (inventory != value)
                {
                    inventory = value;
                    OnPropertyChanged();
                }
            }
        }
        public int FreeInventorySlots => Constants.MAX_INVENTORY_SLOTS - inventory.Count;
        public int GetFirstSlot(int itemId = 0)
        {
            var itemSlot = inventory.Select((item, index) => new { item, index })
                                    .FirstOrDefault(x => x.item.Id == itemId);
            if (itemSlot != null)
                return itemSlot.index;

            if (itemId != 0)
            {
                var emptySlot = inventory.Select((item, index) => new { item, index })
                                         .FirstOrDefault(x => x.item.Id == 0);
                return emptySlot?.index ?? -1;
            }

            return -1;
        }
        public event EventHandler<InventoryChangedEventArgs>? InventoryChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void UpdateInventory()
        {

            var startAddress = Addresses.InventoryStartAddress;

            for (int i = 0; i < Constants.MAX_INVENTORY_SLOTS; i++)
            {
                DarkCloud2Item item = new DarkCloud2Item();

                var foo = Memory.ReadObject<DarkCloud2Item>(startAddress);
                foo.Name = Helpers.ItemList.First(x => x.Id == foo.Id).Name;
                foo.IsProgression = Helpers.ItemList.FirstOrDefault(x => x.Id == foo.Id).IsProgression;

                var itemId = Memory.ReadShort(startAddress);
                item.Id = itemId;
                var itemType = Memory.ReadShort(startAddress - Addresses.ShortOffset);
                item.Type = (DarkCloud2ItemType)itemType;
                item.Name = Helpers.ItemList.First(x => x.Id == item.Id).Name;
                if (item.Type == Enums.DarkCloud2ItemType.Crystal)
                {
                    var itemQuantityAddress = startAddress + 0x00000048;
                    var itemQuantity = Memory.ReadShort(itemQuantityAddress);
                    item.Quantity = (ushort)itemQuantity;
                }
                else
                {
                    var itemQuantityAddress = startAddress + (ulong)Addresses.ItemQuantityOffset;
                    var itemQuantity = Memory.ReadShort(itemQuantityAddress);
                    item.Quantity = (ushort)itemQuantity;
                }
                item.IsProgression = Helpers.ItemList.FirstOrDefault(x => x.Id == itemId).IsProgression;
                startAddress += (ulong)Addresses.ItemSlotSize;
                Inventory[i] =item;
            }

        }
        public PlayerState()
        {
            Inventory = new ObservableCollection<DarkCloud2Item>(Enumerable.Range(0, Constants.MAX_INVENTORY_SLOTS).Select(_ => new DarkCloud2Item()));
            Inventory.CollectionChanged += (obj, args) =>
            {
                List<DarkCloud2Item> newItems = new List<DarkCloud2Item>();
                List<DarkCloud2Item> removedItems = new List<DarkCloud2Item>();
                InventoryChangedEventArgs newArgs = null;
                if(oldInventory != null)
                {
                    for(int i = 0; i < Constants.MAX_INVENTORY_SLOTS; i++)
                    {
                        var oldItem = oldInventory[i];
                        var newItem = Inventory[i];

                        if(oldItem.Id == newItem.Id && oldItem.Quantity == newItem.Quantity)
                        {
                            //No change
                            continue;
                        }
                        else if(newItem.Id == 0 || newItem.Quantity == 0)
                        {
                            // item was removed
                            removedItems.Add(oldItem);
                        }
                        else if(oldItem.Id == 0 && newItem.Id != 0)
                        {
                            //item was added
                            newItems.Add(newItem);
                        }
                        else if (newItem.Id == oldItem.Id && newItem.Quantity != oldItem.Quantity)
                        {
                            // item quantity changed
                            newItems.Add(newItem);
                        }
                        else
                        {
                            // item was replaced
                            newItems.Add(newItem);
                            removedItems.Add(oldItem);
                        }
                    }
                    newArgs = new InventoryChangedEventArgs { NewItems = newItems, RemovedItems = removedItems, IsArchipelagoUpdate = IsReceivingArchipelagoItem };
                }
                else
                {
                    newArgs = new InventoryChangedEventArgs { NewItems = Inventory.ToList(), RemovedItems = new List<DarkCloud2Item>(), IsArchipelagoUpdate = IsReceivingArchipelagoItem };
                }
                if (!newItems.Any() && !removedItems.Any()) 
                {
                    oldInventory = Inventory.ToList();
                    return; 
                }
                InventoryChanged?.Invoke(obj, newArgs);
                IsReceivingArchipelagoItem = false;
                oldInventory = Inventory.ToList();
            };
        }
    }
}
