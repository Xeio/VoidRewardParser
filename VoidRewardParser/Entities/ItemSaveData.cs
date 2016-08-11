using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VoidRewardParser.Entities
{
    [Serializable]
    public class ItemSaveData : INotifyPropertyChanged
    {
        private string _notes;
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
        
        public string Notes
        {
            get
            {
                return _notes;
            }
            set
            {
                if (_notes == value) return;
                _notes = value;
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
