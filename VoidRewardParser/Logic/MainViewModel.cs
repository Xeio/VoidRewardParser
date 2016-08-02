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
        private DateTime _lastMissionComplete;

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
            _parseTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _parseTimer.Tick += _parseTimer_Tick;
            _parseTimer.Start();
        }

        private async void _parseTimer_Tick(object sender, object e)
        {
            if (Warframe.WarframeIsRunning())
            {
                var text = await ScreenCapture.ParseTextAsync();
                var primeData = await FileCacheManager.Instance.GetValue("PrimeData" + LocalizationManager.Language, () => PrimeData.Load());
                PrimeItems = primeData.Primes.Where(p => text.Contains(p.Name)).ToList();

                if (text.Contains(LocalizationManager.MissionCompleteString))
                {
                    OnMissionComplete();
                }
                WarframeNotDetected = false;
            }
            else
            {
                WarframeNotDetected = true;
            }
        }

        private void OnMissionComplete()
        {
            if(_lastMissionComplete + TimeSpan.FromSeconds(30) < DateTime.Now)
            {
                //Only raise this event at most once every 30 seconds
                MissionComplete?.Invoke(this, EventArgs.Empty);
                _lastMissionComplete = DateTime.Now;
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
