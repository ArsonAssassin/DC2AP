using Archipelago.Core.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

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

        private InventorySlot[] _slots;
        private InventorySlot[] _previousSlots;

        public bool IsReceivingArchipelagoItem { get; set; }

        public InventorySlot[] Slots => _slots;

        public int FreeInventorySlots => _slots.Count(s => s.IsEmpty);

        public int GetFirstSlot(int itemId = 0)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].ItemId == itemId)
                    return i;
            }

            if (itemId != 0)
            {
                for (int i = 0; i < _slots.Length; i++)
                {
                    if (_slots[i].IsEmpty)
                        return i;
                }
            }

            return -1;
        }

        public event EventHandler<InventoryChangedEventArgs> InventoryChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateInventory()
        {
            var newSlots = Memory.ReadStructs<InventorySlot>(
                Addresses.InventoryStartAddress, Constants.MAX_INVENTORY_SLOTS).ToArray();

            if (_previousSlots != null)
            {
                var diff = ComputeDiff(_previousSlots, newSlots);
                if (diff.NewItems.Count > 0 || diff.RemovedItems.Count > 0)
                {
                    diff.IsArchipelagoUpdate = IsReceivingArchipelagoItem;
                    InventoryChanged?.Invoke(this, diff);
                    IsReceivingArchipelagoItem = false;
                }
            }

            _previousSlots = (InventorySlot[])newSlots.Clone();
            _slots = newSlots;
        }

        private static InventoryChangedEventArgs ComputeDiff(InventorySlot[] oldSlots, InventorySlot[] newSlots)
        {
            var newItems = new List<DarkCloud2Item>();
            var removedItems = new List<DarkCloud2Item>();

            for (int i = 0; i < Constants.MAX_INVENTORY_SLOTS; i++)
            {
                var oldSlot = oldSlots[i];
                var newSlot = newSlots[i];

                if (oldSlot == newSlot)
                    continue;

                if (newSlot.IsEmpty || newSlot.Quantity == 0)
                {
                    removedItems.Add(oldSlot.ToItem());
                }
                else if (oldSlot.IsEmpty && !newSlot.IsEmpty)
                {
                    newItems.Add(newSlot.ToItem());
                }
                else if (newSlot.ItemId == oldSlot.ItemId && newSlot.Quantity != oldSlot.Quantity)
                {
                    newItems.Add(newSlot.ToItem());
                }
                else
                {
                    // Item was replaced
                    newItems.Add(newSlot.ToItem());
                    removedItems.Add(oldSlot.ToItem());
                }
            }

            return new InventoryChangedEventArgs { NewItems = newItems, RemovedItems = removedItems };
        }

        public PlayerState()
        {
            _slots = new InventorySlot[Constants.MAX_INVENTORY_SLOTS];
        }
    }
}
