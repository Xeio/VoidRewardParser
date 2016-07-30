using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using VoidRewardParser.Entities;

namespace VoidRewardParser.Logic
{
    public class MainViewModel : INotifyPropertyChanged
    {
        DispatcherTimer _parseTimer;
        private List<PrimeItem> _primeItems = new List<PrimeItem>();
        private List<PrimeItem> _rawPrimes;

        private async Task<List<PrimeItem>> GetRawPrimes()
        {
            return _rawPrimes ?? (_rawPrimes = await PrimeData.Load());
        }

        public List<PrimeItem> PrimeItems
        {
            get
            {
                return _primeItems;
            }

            set
            {
                if (_primeItems == value) return;
                _primeItems = value;
                OnNotifyPropertyChanged();
            }
        }

        public MainViewModel()
        {
            _parseTimer = new DispatcherTimer();
            _parseTimer.Interval = TimeSpan.FromMilliseconds(500);
            _parseTimer.Tick += _parseTimer_Tick;
            _parseTimer.Start();
        }

        private async void _parseTimer_Tick(object sender, object e)
        {
            if (Warframe.WarframeIsRunning())
            {
                var text = await ScreenCapture.ParseTextAsync();
                if (text.Contains("VOID MISSION COMPLETE") &&
                    text.Contains("SELECT A REWARD"))
                {
                    var primes = await GetRawPrimes();
                    PrimeItems = primes.Where(p => text.Contains(p.Name.ToUpper())).ToList();
                }
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnNotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
