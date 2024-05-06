using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP.Models
{
    public class Enemy
    {
        public string Name { get; set; }
        public string ModelAI { get; set; }
        public string ModelType { get; set; }
        public string Sound { get; set; }
        public string Unknown1 { get; set; }
        public string HP { get; set; }
        public string Family { get; set; }
        public string ABS { get; set; }
        public string Gilda { get; set; }
        public string Unknown2 { get; set; }
        public string Rage { get; set; }
        public string Unknown3 { get; set; }
        public string Damage { get; set; }
        public string Defense { get; set; }
        public string BossFlag { get; set; }
        public string Weaknesses { get; set; }
        public string Effectiveness { get; set; }
        public string Unknown4 { get; set; }
        public string IsRidepodEnemy { get; set; }
        public string UnusedBits { get; set; }
        public string Minions { get; set; }
        public List<ItemId> Items { get; set; }
        public string Unknown6 { get; set; }
        public string Dungeon { get; set; }
        public string BestiarySpot { get; set; }
        public string SharedHP { get; set; }
        public string Unknown7 { get; set; }
    }
}
