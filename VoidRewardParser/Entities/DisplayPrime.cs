using Prism.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VoidRewardParser.Entities
{
    public class DisplayPrime : INotifyPropertyChanged
    {
        private PrimeItem _prime;
        private ItemSaveData _data;
        private bool _visible;
        private string _platinumPrice;

        public PrimeItem Prime
        {
            get
            {
                return _prime;
            }
            set
            {
                if (_prime == value) return;
                _prime = value;
                OnNotifyPropertyChanged();
            }
        }

        public ItemSaveData Data
        {
            get
            {
                return _data;
            }
            set
            {
                if (_data == value) return;
                _data = value;
                OnNotifyPropertyChanged();
            }
        }

        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                if (_visible == value) return;
                _visible = value;
                OnNotifyPropertyChanged();
            }
        }

        public string PlatinumPrice
        {
            get
            {
                if (_platinumPrice == null)
                {
                    return "...";
                }

                return _platinumPrice;
            }
            set
            {
                if (_platinumPrice == value) return;
                _platinumPrice = value;
                OnNotifyPropertyChanged();
            }
        }

        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SubtractCommand { get; set; }

        public DisplayPrime()
        {
            AddCommand = new DelegateCommand(() => { Data.NumberOwned++; });
            SubtractCommand = new DelegateCommand(() => { Data.NumberOwned--; });
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
