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
    public class InventoryChangedEventArgs : NotifyCollectionChangedEventArgs
    {
        public InventoryChangedEventArgs(NotifyCollectionChangedAction action, object item = null)    : base(action, item, action == NotifyCollectionChangedAction.Reset ? -1 : 0)
        {
        }
        public InventoryChangedEventArgs(NotifyCollectionChangedAction action, object item, int index)
    : base(action, item, index)
        {
        }
        public InventoryChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
    : base(action, newItem, oldItem, index)
        {
        }
        public InventoryChangedEventArgs(NotifyCollectionChangedAction action, object item, int newIndex, int oldIndex)
    : base(action, item, newIndex, oldIndex)
        {
        }
        public bool IsArchipelagoUpdate { get; set; }
    }
}
