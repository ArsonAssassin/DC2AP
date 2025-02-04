using Archipelago.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP.Models
{
    public class Enemy
    {
        [MemoryOffset(0x000, stringLength: 32)]
        public string Name { get; set; }
        [MemoryOffset(0x020, stringLength: 32)]
        public string ModelAI { get; set; }

        [MemoryOffset(0x40)]
        public int ModelType { get; set; }
        [MemoryOffset(0x044)]
        public int Sound { get; set; }
        [MemoryOffset(0x48)]
        public int Unknown1 { get; set; }

        [MemoryOffset(0x4C)]
        public int HP { get; set; }

        [MemoryOffset(0x50)]
        public short Family { get; set; }

        [MemoryOffset(0x52)]
        public short ABS { get; set; }

        [MemoryOffset(0x54)]
        public short Gilda { get; set; }

        [MemoryOffset(0x56, stringLength: 6)]
        public string Unknown2 { get; set; }

        [MemoryOffset(0x5C)]
        public short Rage { get; set; }

        [MemoryOffset(0x5E, stringLength: 4)]
        public string Unknown3 { get; set; }

        [MemoryOffset(0x62)]
        public short Damage { get; set; }

        [MemoryOffset(0x64)]
        public short Defense { get; set; }

        [MemoryOffset(0x66)]
        public short BossFlag { get; set; }

        [MemoryOffset(0x68, stringLength: 16)]
        public string Weaknesses { get; set; }

        [MemoryOffset(0x78, stringLength: 24)]
        public string Effectiveness { get; set; }

        [MemoryOffset(0x90, stringLength: 4)]
        public string Unknown4 { get; set; }

        [MemoryOffset(0x94)]
        public byte IsRidepodEnemy { get; set; }

        [MemoryOffset(0x96, stringLength: 2)]
        public string UnusedBits { get; set; }

        [MemoryOffset(0x98, stringLength: 4)]
        public string Minions { get; set; }

        [MemoryOffset(0x9C)]
        public short ItemSlot1 { get; set; }

        [MemoryOffset(0x9E)]
        public short ItemSlot2 { get; set; }

        [MemoryOffset(0xA0)]
        public short ItemSlot3 { get; set; }
        [MemoryOffset(0xA2, stringLength: 10)]
        public string Unknown6 { get; set; }

        [MemoryOffset(0xAC)]
        public short DungeonId { get; set; }

        [MemoryOffset(0xAE)]
        public short BestiarySpot { get; set; }

        [MemoryOffset(0xB0)]
        public short SharedHP { get; set; }

        [MemoryOffset(0xB2)]
        public short Unknown7 { get; set; }

    }
}
