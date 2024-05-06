using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP.Models
{
    public class Dungeon
    {
        public int id { get; set; }
        public string Name { get; set; }
        public int FloorCount { get; set; }
        public List<Floor> Floors { get; set; }
    }
}
