using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archipelago.PCSX2.Models
{
    public class GameState
    {
        public GameState()
        {
            CompletedLocations = new List<Location>();
            ReceivedItems = new List<Item>();
            CustomValues = new Dictionary<string, object>();
        }
        public List<Location> CompletedLocations { get; set; }
        public List<Item> ReceivedItems { get; set; }
        public Dictionary<string, object> CustomValues { get; set; }
    }
}
