using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP.Models
{
    public class GameState : INotifyPropertyChanged
    {
        private int currentDungeon;
        private int currentFloor;

        public int CurrentDungeon
        {
            get => currentDungeon;
            set
            {
                if (currentDungeon != value)
                {
                    currentDungeon = value;
                    OnPropertyChanged();
                }
            }
        }
        public int CurrentFloor
        {
            get => currentFloor;
            set
            {
                if (currentFloor != value)
                {
                    currentFloor = value;
                    OnPropertyChanged();
                }
            }
        }


        public GameState()
        {

        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
