using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP.Models
{
    public class Floor
    {
        public string IsUnlocked { get; set; }
        public string IsFinished { get; set; }
        public string SpecialMedalCompleted { get; set; }
        public string ClearMedalCompleted { get; set; }
        public string FishMedalCompleted { get; set; }
        public string SphedaMedalCompleted { get; set; }
        public string KilledAllMonsters { get; set; }
        public string GotGeostone { get; set; }
        public string DownloadedGeostone { get; set; }
        public int MonstersKilled { get; set; }
        public int TimesVisited { get; set; }
    }
}
