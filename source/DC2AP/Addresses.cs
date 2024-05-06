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
        private int currentFloorUS = 0x21ECD638;
        private int currentFloorPAL = 0x21EFC658;
        private int currentDungeonUS = 0x20376638;
        private int currentDungeonPAL = 0x2037C828;
        private int[] dungeonAreaChestAddressUS = { 0x20E656A0, 0x20E65D60, 0x20E67220, 0x20E67620, 0x20E681A0, 0x20E679A0, 0x20E68960 };
        private int[] dungeonAreaChestAddressPAL = { 0x20E8A520, 0x20E8ABE0, 0x20E8C0A0, 0x20E8C4A0, 0x20E8D020, 0x20E8C820, 0x20E8D7E0 };
        private int dungeonCheckAddressUS = 0x21E9F6E0;
        private int dungeonCheckAddressPAL = 0x21ECE1E0;

        public int DungeonStartAddress = 0x21E1DE22;

        public int exitFlagUS = 0x20364BD0;
        public int exitFlagPAL = 0x203694D0;
        public int exitFlagCheck = 1701667175;

        public int CurrentFloor;
        public int CurrentDungeon;
        public int PreviousFloor;
        public int[] DungeonAreaChestAddress;
        public int DungeonCheckAddress;
        public int CurrentExitFlag;


        public int EnemyStartAddress = 0x2033D9E0;

        public int[] tier1weapons = { 1, 2, 9, 15, 22, 23, 24, 90 };
        public int[] tier2weapons = { 10, 11, 18, 25, 28, 31 };
        public int[] tier3weapons = { 3, 12, 17, 26, 35 };
        public int[] tier4weapons = { 4, 6, 13, 29, 32, 36 };
        public int[] tier5weapons = { 5, 7, 14, 16, 27, 37 };
        public int[] tier6weapons = { 8, 19, 30, 33, 38 };
        public int[] tier7weapons = { 20, 21, 34, 39, 40 };

        public int PlayerGilda = 0x21E6384C;
        public int PlayerMedals = 0x21E63850;
        public int InventoryStartAddress = 0x21E1EAB2;
    }
}
