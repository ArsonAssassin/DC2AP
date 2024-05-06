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

        public event NotifyCollectionChangedEventHandler? InventoryChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public PlayerState()
        {
            inventory = new ObservableCollection<Item>();
            inventory.CollectionChanged += (obj, args) =>
            {
                InventoryChanged?.Invoke(obj, args);
            };
        }
    }
}
