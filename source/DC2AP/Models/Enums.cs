using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP.Models
{
    public class Enums
    {
        public enum DarkCloud2ItemType 
        {
            Item = 1,
            Crystal = 2,
            Weapon = 3,
            StevePart = 5

        }

        public enum DarkCloud2ItemCategory
        {
            Weapon = 1,
            RidepodCore = 11,
            Element = 16,
            GeoramaResource = 20,
            Consumable = 22,
            Throwable = 23,
            RepairPowder = 24,
            KeyItem = 27,
            MedalHolder = 33
        }
    }
}
