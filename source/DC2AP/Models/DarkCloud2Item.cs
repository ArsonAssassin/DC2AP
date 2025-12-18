using Archipelago.Core.Models;
using Archipelago.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DC2AP.Models.Enums;

namespace DC2AP.Models
{
    public class DarkCloud2Item : Item
    {
        [MemoryOffset(0x00)]
        public new short Id { get; set; }
        public ushort Quantity
        {
            get => Type == DarkCloud2ItemType.Crystal ? CrystalQuantity : ItemQuantity;
            set
            {
                if (Type == DarkCloud2ItemType.Crystal)
                    CrystalQuantity = value;
                else
                    ItemQuantity = value;
            }
        }
        [MemoryOffset(0x02)]
        public DarkCloud2ItemType Type {  get; set; }
        [MemoryOffset(0x0F)]
        public ushort ItemQuantity {  get; set; }
        [MemoryOffset(0x48)]
        public ushort CrystalQuantity { get; set; }
    }
}
