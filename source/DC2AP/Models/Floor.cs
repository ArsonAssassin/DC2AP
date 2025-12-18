using Archipelago.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP.Models
{
    //public class Floor
    //{
    //    public string IsUnlocked { get; set; }
    //    public string IsFinished { get; set; }
    //    public string SpecialMedalCompleted { get; set; }
    //    public string ClearMedalCompleted { get; set; }
    //    public string FishMedalCompleted { get; set; }
    //    public string SphedaMedalCompleted { get; set; }
    //    public string KilledAllMonsters { get; set; }
    //    public string GotGeostone { get; set; }
    //    public string DownloadedGeostone { get; set; }
    //    public int MonstersKilled { get; set; }
    //    public int TimesVisited { get; set; }
    //}
	
	public class Floor
	{
		// First byte with bit flags
		[MemoryOffset(0x00, bitPosition: 0)]
		public bool Unlocked { get; set; }
		
		[MemoryOffset(0x00, bitPosition: 1)]
		public bool Completed { get; set; }
		
		[MemoryOffset(0x00, bitPosition: 3)]
		public bool SpecialMedalCompleted { get; set; } // Special
		
		[MemoryOffset(0x00, bitPosition: 4)]
		public bool ClearMedalCompleted { get; set; } // Clear
		
		[MemoryOffset(0x00, bitPosition: 5)]
		public bool FishMedalCompleted { get; set; } // Fish
		
		[MemoryOffset(0x00, bitPosition: 7)]
		public bool SphedaMedalCompleted { get; set; } // Spheda
		
		// Second byte with bit flags (optional bits 0 and 1)
		[MemoryOffset(0x01, bitPosition: 0)]
		public bool GotGeostone { get; set; }
		
		[MemoryOffset(0x01, bitPosition: 1)]
		public bool DownloadedGeostone { get; set; }
		
		[MemoryOffset(0x01, bitPosition: 2)]
		public bool KilledAllMonsters { get; set; }
		
		// Next 2 bytes: ushort for monsters killed
		[MemoryOffset(0x02)]
		public ushort MonstersKilled { get; set; }
		
		// Next 2 bytes: ushort for times visited
		[MemoryOffset(0x04)]
		public ushort TimesVisited { get; set; }
		
		// Unknown data section (14 bytes)
		[MemoryOffset(0x06, stringLength:7)]
		public string UnknownData { get; set; }
	}
	
}
