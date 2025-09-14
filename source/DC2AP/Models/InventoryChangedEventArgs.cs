using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using static System.Collections.Specialized.BitVector32;
using System.Reflection;

namespace DC2AP.Models
{
    public class InventoryChangedEventArgs : EventArgs
    {
        public List<DarkCloud2Item> NewItems { get; set; } = new List<DarkCloud2Item>();
        public List<DarkCloud2Item> RemovedItems { get; set; } = new List<DarkCloud2Item>();
        public bool IsArchipelagoUpdate { get; set; }
        public InventoryChangedEventArgs() { }
    }
}
