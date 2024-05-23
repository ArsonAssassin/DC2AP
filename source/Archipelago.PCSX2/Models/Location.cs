using Archipelago.PCSX2.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archipelago.PCSX2.Models
{
    public class Location
    {
        [JsonConverter(typeof(HexToIntConverter))]
        public int Address { get; set; }
        public int AddressBit { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public LocationCheckType CheckType { get; set; }
        public string CheckValue { get; set; }
    }
}
