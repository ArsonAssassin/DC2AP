using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP.Models
{
    public class ArchipelagoOptions
    {
        public int ExpMultiplier { get; set; } = 1;
        public int GoldMultiplier { get; set; } = 1;
        public int ResourcePackCount { get; set; } = 0;
        public int WeaponUpgradePackCount { get; set; } = 0;
        public int ElementPackCount { get; set; } = 0;
        public int DungeonCountGoal { get; set; } = 4;
        public bool Fishsanity { get; set; } = false;
        public bool Sphedasanity { get; set; } = false;
        public bool Medalsanity { get; set; } = false;
        public bool Georamasanity { get; set; } = false;
        public bool Photosanity { get; set; } = false;
        public bool Inventionsanity { get; set; } = false;
    }
}
