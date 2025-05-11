using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP
{
    public static class Addresses
    {

        public static ulong PCSX2Offset;

        public static ulong ShortOffset = 0x02;
        public static ulong IntOffset = 0x04;

        public static ulong CurrentFloor = 0x01ECD638;
        public static ulong CurrentDungeon = 0x00376638;
        public static ulong[] DungeonAreaChestAddress = { 0x00E656A0, 0x00E65D60, 0x00E67220, 0x00E67620, 0x00E681A0, 0x00E679A0, 0x00E68960 };
        public static ulong DungeonCheckAddress = 0x01E9F6E0;

        public static ulong DungeonStartAddress = 0x01E1DE22;

        public static ulong ExitFlag = 0x00364BD0;

        public static ulong PreviousFloor;
        public static ulong CurrentExitFlag;


        public static ulong EnemyStartAddress = 0x0033D9E0;

        public static uint[] tier1weapons = { 1, 2, 9, 15, 22, 23, 24, 90 };
        public static uint[] tier2weapons = { 10, 11, 18, 25, 28, 31 };
        public static uint[] tier3weapons = { 3, 12, 17, 26, 35 };
        public static uint[] tier4weapons = { 4, 6, 13, 29, 32, 36 };
        public static uint[] tier5weapons = { 5, 7, 14, 16, 27, 37 };
        public static uint[] tier6weapons = { 8, 19, 30, 33, 38 };
        public static uint[] tier7weapons = { 20, 21, 34, 39, 40 };

        public static ulong PlayerGilda = 0x01E6384C;
        public static ulong PlayerMedals = 0x01E63850;
        public static ulong InventoryStartAddress = 0x01E1EAB2;

    }
}
