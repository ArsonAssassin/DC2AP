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
        private bool isUpdating = false;

        public bool IsReceivingArchipelagoItem { get; set; }
        public ObservableCollection<DarkCloud2Item> Inventory
        {
            get
            {
                if (!isUpdating && inventory.All(x => x.Quantity == 0))
                {
                    UpdateInventory();
                }
                return inventory;
            }
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
            var itemSlot = Inventory.Select((item, index) => new { item, index })
                                    .FirstOrDefault(x => x.item.ItemId == itemId);
            if (itemSlot != null)
                return itemSlot.index;

            if (itemId != 0)
            {
                var emptySlot = inventory.Select((item, index) => new { item, index })
                                         .FirstOrDefault(x => x.item.ItemId == 0);
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
            isUpdating = true;
            try
            {
                var startAddress = Addresses.InventoryStartAddress;

                for (int i = 0; i < Constants.MAX_INVENTORY_SLOTS; i++)
                {
                    var item = Memory.ReadObject<DarkCloud2Item>(startAddress);
                    var itemLookup = Helpers.ItemList.First(x => x.Id == item.ItemId);
                    if (item.Type != DarkCloud2ItemType.Weapon)
                    {
                        item.Name = itemLookup.Name;
                    }
                    item.IsProgression = itemLookup.IsProgression;
                    if (item.ItemId == 90)
                    {
                        Console.Write("");
                        var item2 = Memory.ReadObject<DarkCloud2Item>(startAddress);
                    }
                    startAddress += (ulong)Addresses.ItemSlotSize;
                    Inventory[i] = item;

                }
            }
            finally
            {
                isUpdating = false;
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

                        if(oldItem.ItemId == newItem.ItemId && oldItem.Quantity == newItem.Quantity)
                        {
                            //No change
                            continue;
                        }
                        else if(newItem.ItemId == 0 || newItem.Quantity == 0)
                        {
                            // item was removed
                            removedItems.Add(oldItem);
                        }
                        else if(oldItem.ItemId == 0 && newItem.ItemId != 0)
                        {
                            //item was added
                            newItems.Add(newItem);
                        }
                        else if (newItem.ItemId == oldItem.ItemId && newItem.Quantity != oldItem.Quantity)
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
