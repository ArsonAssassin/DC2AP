using Archipelago.Core.Models;
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
        public bool isProgression { get; set; }
        public long locationId { get; set; }
        public ushort Quantity { get; set; }
        public DarkCloud2ItemType Type {  get; set; }
    }
}
