using Archipelago.Core.Util;
using DC2AP.Helpers;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using static DC2AP.Models.Enums;

namespace DC2AP.Models
{
    [StructLayout(LayoutKind.Explicit, Size = 0x6C)]
    public struct InventorySlot : IEquatable<InventorySlot>
    {
        // ===== COMMON (all item types) =====
        [FieldOffset(0x00)] public short RawType;
        [FieldOffset(0x02)] public short ItemId;
        [FieldOffset(0x04)] public short RawCategory;
        [FieldOffset(0x06)] public short NameChangeFlag;

        // ===== ITEM / CONSUMABLE =====
        [FieldOffset(0x10)] public short ItemQuantity;

        // ===== WEAPON / ROD (overlaps ItemQuantity at 0x10) =====
        [FieldOffset(0x10)] public float MaxDurability;
        [FieldOffset(0x14)] public float CurrentDurability;
        [FieldOffset(0x18)] public float RequiredExp;
        [FieldOffset(0x1C)] public float CurrentExp;
        [FieldOffset(0x20)] public short Level;
        [FieldOffset(0x22)] public short Attack;
        [FieldOffset(0x24)] public short Durable;

        // ===== WEAPON ELEMENTS =====
        [FieldOffset(0x26)] public short Flame;
        [FieldOffset(0x28)] public short Chill;
        [FieldOffset(0x2A)] public short Lightning;
        [FieldOffset(0x2C)] public short Cyclone;
        [FieldOffset(0x2E)] public short Smash;
        [FieldOffset(0x30)] public short Exorcism;
        [FieldOffset(0x32)] public short Beast;
        [FieldOffset(0x34)] public short Scale;
        [FieldOffset(0x3C)] public short SynthesisPoints;

        // ===== FISHING ROD (same offsets as weapon elements) =====
        [FieldOffset(0x26)] public short RodFlight;
        [FieldOffset(0x28)] public short RodStrength;
        [FieldOffset(0x2C)] public short RodGrip;
        [FieldOffset(0x30)] public short RodResilience;
        [FieldOffset(0x3C)] public short FishingPoints;

        // ===== FISH =====
        [FieldOffset(0x28)] public short FishSize;

        // ===== CRYSTAL =====
        [FieldOffset(0x48)] public short CrystalQuantity;

        // ===== HELPERS =====

        public readonly DarkCloud2ItemType Type => (DarkCloud2ItemType)RawType;

        public readonly DarkCloud2ItemCategory Category => (DarkCloud2ItemCategory)RawCategory;

        public readonly bool IsEmpty => ItemId == 0;

        public short Quantity
        {
            readonly get => Type == DarkCloud2ItemType.Crystal ? CrystalQuantity : ItemQuantity;
            set
            {
                if (Type == DarkCloud2ItemType.Crystal)
                    CrystalQuantity = value;
                else
                    ItemQuantity = value;
            }
        }

        /// <summary>
        /// Reads the weapon/rod name from game memory for this slot.
        /// For non-weapon types, use ItemList lookup instead.
        /// </summary>
        public static string ReadWeaponName(ulong slotAddress)
        {
            return Memory.ReadString(slotAddress + 0x43, 41);
        }

        /// <summary>
        /// Converts this slot to a DarkCloud2Item domain model.
        /// </summary>
        public readonly DarkCloud2Item ToItem()
        {
            var item = new DarkCloud2Item
            {
                ItemId = ItemId,
                RawType = RawType,
                RawCategory = RawCategory,
                NameChangeFlag = NameChangeFlag,
                ItemQuantity = ItemQuantity,
                CrystalQuantity = CrystalQuantity,
            };

            if (Type == DarkCloud2ItemType.Weapon || Type == DarkCloud2ItemType.Fish)
            {
                item.MaxDurability = MaxDurability;
                item.CurrentDurability = CurrentDurability;
                item.RequiredExp = RequiredExp;
                item.CurrentExp = CurrentExp;
                item.Level = Level;
                item.Attack = Attack;
                item.Durable = Durable;
                item.Flame = Flame;
                item.Chill = Chill;
                item.Lightning = Lightning;
                item.Cyclone = Cyclone;
                item.Smash = Smash;
                item.Exorcism = Exorcism;
                item.Beast = Beast;
                item.Scale = Scale;
                item.SynthesisPoints = SynthesisPoints;
            }

            // Resolve name from item list; weapons keep their in-memory name
            var id = ItemId;
            var lookup = GeneralHelpers.ItemList?.FirstOrDefault(x => x.Id == id);
            if (lookup != null)
            {
                item.Name = lookup.Name;
            }

            return item;
        }

        public readonly bool Equals(InventorySlot other)
        {
            if (ItemId != other.ItemId) return false;
            if (RawType != other.RawType) return false;

            return Type switch
            {
                DarkCloud2ItemType.Crystal => CrystalQuantity == other.CrystalQuantity,
                DarkCloud2ItemType.Weapon => Level == other.Level
                    && Attack == other.Attack
                    && MaxDurability == other.MaxDurability,
                _ => ItemQuantity == other.ItemQuantity,
            };
        }

        public override readonly bool Equals(object obj) => obj is InventorySlot other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(ItemId, RawType);

        public static bool operator ==(InventorySlot left, InventorySlot right) => left.Equals(right);
        public static bool operator !=(InventorySlot left, InventorySlot right) => !left.Equals(right);
    }
}
