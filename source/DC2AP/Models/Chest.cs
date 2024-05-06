using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP.Models
{
    public class Chest : INotifyPropertyChanged
    {
        public int Item1 { get; set; }
        public int Quantity1 { get; set; }
        public int Item2 { get; set; }
        public int Quantity2 { get; set; }
        public bool IsDoubleChest { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
