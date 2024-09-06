using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP
{
    public class Addresses
    {
        private static Addresses instance = null;

        public static Addresses Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Addresses(Program.GameVersion);
                }
                return instance;
            }
        }
        private Addresses(string GameVersion)
        {
            if (GameVersion == "PAL")
            {
                CurrentFloor = currentFloorPAL;
                CurrentDungeon = currentDungeonPAL;
                DungeonAreaChestAddress = dungeonAreaChestAddressPAL;
                DungeonCheckAddress = dungeonCheckAddressPAL;
                CurrentExitFlag = exitFlagPAL;
            }
            else if (GameVersion == "US")
            {
                CurrentFloor = currentFloorUS;
                CurrentDungeon = currentDungeonUS;
                DungeonAreaChestAddress = dungeonAreaChestAddressUS;
                DungeonCheckAddress = dungeonCheckAddressUS;
                CurrentExitFlag = exitFlagUS;
            }
        }

        public uint ShortOffset = 0x00000002;
        public uint IntOffset = 0x00000004;

        private uint currentFloorUS = 0x21ECD638;
        private uint currentFloorPAL = 0x21EFC658;
        private uint currentDungeonUS = 0x20376638;
        private uint currentDungeonPAL = 0x2037C828;
        private uint[] dungeonAreaChestAddressUS = { 0x20E656A0, 0x20E65D60, 0x20E67220, 0x20E67620, 0x20E681A0, 0x20E679A0, 0x20E68960 };
        private uint[] dungeonAreaChestAddressPAL = { 0x20E8A520, 0x20E8ABE0, 0x20E8C0A0, 0x20E8C4A0, 0x20E8D020, 0x20E8C820, 0x20E8D7E0 };
        private uint dungeonCheckAddressUS = 0x21E9F6E0;
        private uint dungeonCheckAddressPAL = 0x21ECE1E0;

        public uint DungeonStartAddress = 0x21E1DE22;

        public uint exitFlagUS = 0x20364BD0;
        public uint exitFlagPAL = 0x203694D0;

        public uint CurrentFloor;
        public uint CurrentDungeon;
        public uint PreviousFloor;
        public uint[] DungeonAreaChestAddress;
        public uint DungeonCheckAddress;
        public uint CurrentExitFlag;


        public uint EnemyStartAddress = 0x2033D9E0;

        public uint[] tier1weapons = { 1, 2, 9, 15, 22, 23, 24, 90 };
        public uint[] tier2weapons = { 10, 11, 18, 25, 28, 31 };
        public uint[] tier3weapons = { 3, 12, 17, 26, 35 };
        public uint[] tier4weapons = { 4, 6, 13, 29, 32, 36 };
        public uint[] tier5weapons = { 5, 7, 14, 16, 27, 37 };
        public uint[] tier6weapons = { 8, 19, 30, 33, 38 };
        public uint[] tier7weapons = { 20, 21, 34, 39, 40 };

        public uint PlayerGilda = 0x21E6384C;
        public uint PlayerMedals = 0x21E63850;
        public uint InventoryStartAddress = 0x21E1EAB2;


        public class StoryFlags
        {
            
        }
    }
}
