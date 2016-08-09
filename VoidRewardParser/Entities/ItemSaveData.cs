using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VoidRewardParser.Entities
{
    [Serializable]
    public class ItemSaveData : INotifyPropertyChanged
    {
        private int _numberOwned;

        public int NumberOwned
        {
            get
            {
                return _numberOwned;
            }
            set
            {
                if (_numberOwned == value) return;
                _numberOwned = value;
                OnNotifyPropertyChanged();
            }
        }
        
        #region INotifyPropertyChanged

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
