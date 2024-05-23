using Archipelago.PCSX2.Models;
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
        public event NotifyCollectionChangedEventHandler? InventoryChanged;
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
                InventoryChanged?.Invoke(obj, args);
            };
        }
    }
}
