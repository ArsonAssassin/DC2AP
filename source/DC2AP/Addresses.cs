using DC2AP.WinUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP
{
    public static class Addresses
    {

        public static uint ShortOffset = 0x02;
        public static uint IntOffset = 0x04;

        public static uint CurrentFloor = 0x21ECD638;
        public static uint CurrentDungeon = 0x20376638;
        public static uint[] DungeonAreaChestAddress = { 0x20E656A0, 0x20E65D60, 0x20E67220, 0x20E67620, 0x20E681A0, 0x20E679A0, 0x20E68960 };
        public static uint DungeonCheckAddress = 0x21E9F6E0;

        public static uint DungeonStartAddress = 0x21E1DE22;

        public static uint ExitFlag = 0x20364BD0;

        public static uint PreviousFloor;
        public static uint CurrentExitFlag;


        public static uint EnemyStartAddress = 0x2033D9E0;

        public static uint[] tier1weapons = { 1, 2, 9, 15, 22, 23, 24, 90 };
        public static uint[] tier2weapons = { 10, 11, 18, 25, 28, 31 };
        public static uint[] tier3weapons = { 3, 12, 17, 26, 35 };
        public static uint[] tier4weapons = { 4, 6, 13, 29, 32, 36 };
        public static uint[] tier5weapons = { 5, 7, 14, 16, 27, 37 };
        public static uint[] tier6weapons = { 8, 19, 30, 33, 38 };
        public static uint[] tier7weapons = { 20, 21, 34, 39, 40 };

        public static uint PlayerGilda = 0x21E6384C;
        public static uint PlayerMedals = 0x21E63850;
        public static uint InventoryStartAddress = 0x21E1EAB2;

    }
}
