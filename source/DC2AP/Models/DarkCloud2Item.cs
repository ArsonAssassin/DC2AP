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
        // ===== COMMON PROPERTIES (All item types) =====

        [MemoryOffset(0x00)]
        public short RawType {  get; set; }
        public DarkCloud2ItemType Type { get => (DarkCloud2ItemType)Enum.ToObject(typeof(DarkCloud2ItemType), RawType); }

        [MemoryOffset(0x02)]
        public new short Id { get; set; }

        [MemoryOffset(0x04)]
        public short RawCategory { get; set; }
        public new DarkCloud2ItemCategory Category { get => (DarkCloud2ItemCategory)Enum.ToObject(typeof(DarkCloud2ItemCategory), RawCategory); }

        [MemoryOffset(0x06)]
        public short NameChangeFlag { get; set; }

        // ===== ITEM/CRYSTAL PROPERTIES =====
        [MemoryOffset(0x10)]
        public short ItemQuantity { get; set; }

        [MemoryOffset(0x48)]
        public short CrystalQuantity { get; set; }

        public short Quantity
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

        // ===== FISH PROPERTIES =====
        [MemoryOffset(0x28)]
        public short FishSize { get; set; }

        // ===== WEAPON/FISHING ROD COMMON PROPERTIES =====
        [MemoryOffset(0x10)]
        public float MaxDurability { get; set; }

        [MemoryOffset(0x14)]
        public float CurrentDurability { get; set; }

        [MemoryOffset(0x18)]
        public float RequiredExp { get; set; }

        [MemoryOffset(0x1C)]
        public float CurrentExp { get; set; }

        [MemoryOffset(0x20)]
        public short Level { get; set; }

        [MemoryOffset(0x22)]
        public short Attack { get; set; }

        [MemoryOffset(0x24)]
        public short Durable { get; set; }

        [MemoryOffset(0x43, stringLength: 50)]
        public string Name { get; set; }

        // ===== WEAPON PROPERTIES =====
        [MemoryOffset(0x26)]
        public short Flame { get; set; }

        [MemoryOffset(0x28)]
        public short Chill { get; set; }

        [MemoryOffset(0x2A)]
        public short Lightning { get; set; }

        [MemoryOffset(0x2C)]
        public short Cyclone { get; set; }

        [MemoryOffset(0x2E)]
        public short Smash { get; set; }

        [MemoryOffset(0x30)]
        public short Exorcism { get; set; }

        [MemoryOffset(0x32)]
        public short Beast { get; set; }

        [MemoryOffset(0x34)]
        public short Scale { get; set; }

        [MemoryOffset(0x3C)]
        public short SynthesisPoints { get; set; }

        // ===== FISHING ROD PROPERTIES (Same offsets as weapon elementals) =====
        [MemoryOffset(0x26)]
        public short RodFlight { get; set; }

        [MemoryOffset(0x28)]
        public short RodStrength { get; set; }

        [MemoryOffset(0x30)]
        public short RodResilience { get; set; }

        [MemoryOffset(0x2C)]
        public short RodGrip { get; set; }

        [MemoryOffset(0x3C)]
        public short FishingPoints { get; set; }

        [MemoryOffset(0x00, byteArrayLength: 110)]
        public byte[] RawData { get; set; }
    }
}
