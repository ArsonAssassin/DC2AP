using Archipelago.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP.Models
{
    public class PlayerState : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int Gilda { get; set; }
        public int MedalCount { get; set; }
        private ObservableCollection<Item> inventory;
        public bool IsReceivingArchipelagoItem { get; set; }
        public ObservableCollection<Item> Inventory
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
        public int FreeInventorySlots => 144 - inventory.Count;
        public int GetFirstSlot(int itemId = 0)
        {
            if (inventory.Any(x => x.Id == itemId))
            {
                return inventory.IndexOf(inventory.First(x => x.Id == itemId));
            }
            else if (itemId != 0)
            {
                if (inventory.Any(x => x.Id == 0))
                {
                    return inventory.IndexOf(inventory.First(x => x.Id == 0));
                }
            }
            return -1;
        }
        public event EventHandler<InventoryChangedEventArgs>? InventoryChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public PlayerState()
        {
            inventory = new ObservableCollection<Item>(Enumerable.Range(0, 144).Select(_ => new Item()));
            inventory.CollectionChanged += (obj, args) =>
            {
                InventoryChangedEventArgs newArgs = null;
                if (((args.Action == NotifyCollectionChangedAction.Add || args.Action == NotifyCollectionChangedAction.Remove) && args.NewStartingIndex == null) || args.Action == NotifyCollectionChangedAction.Reset)
                {
                    for (int i = 0; i < args.NewItems.Count; i++)
                    {
                        newArgs = new InventoryChangedEventArgs(args.Action, args.NewItems[i]);
                    }
                }
                else if (args.Action == NotifyCollectionChangedAction.Add || args.Action == NotifyCollectionChangedAction.Remove)
                {
                    for (int i = 0; i < args.NewItems.Count; i++)
                    {
                        newArgs = new InventoryChangedEventArgs(args.Action, args.NewItems[i], args.NewStartingIndex);
                    }
                }
                else if (args.Action == NotifyCollectionChangedAction.Replace)
                {
                    for (int i = 0; i < args.NewItems.Count; i++)
                    {
                        newArgs = new InventoryChangedEventArgs(args.Action, args.NewItems[i], args.OldItems[i], args.NewStartingIndex);
                    }
                }
                else if (args.Action == NotifyCollectionChangedAction.Move)
                {
                    for (int i = 0; i < args.NewItems.Count; i++)
                    {
                        newArgs = new InventoryChangedEventArgs(args.Action, args.NewItems[i], args.NewStartingIndex, args.OldStartingIndex);
                    }
                }
                if(newArgs == null) return;
                newArgs.IsArchipelagoUpdate = IsReceivingArchipelagoItem;

                InventoryChanged?.Invoke(obj, newArgs);
                IsReceivingArchipelagoItem = false;

            };
        }
    }
}
