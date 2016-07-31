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
        private bool _warframeNotDetected;

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

        public bool WarframeNotDetected
        {
            get
            {
                return _warframeNotDetected;
            }
            set
            {
                if (_warframeNotDetected == value) return;
                _warframeNotDetected = value;
                OnNotifyPropertyChanged();
            }
        }

        public event EventHandler MissionComplete;

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
                    var primeData = await FileCacheManager.Instance.GetValue("PrimeData", () => PrimeData.Load());
                    PrimeItems = primeData.Primes.Where(p => text.Contains(p.Name.ToUpper())).ToList();

                    _parseTimer.Stop();
                    MissionComplete?.Invoke(this, EventArgs.Empty);
                    await Task.Delay(30000);
                    _parseTimer.Start();
                }
                WarframeNotDetected = false;
            }
            else
            {
                WarframeNotDetected = true;
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
