using Newtonsoft.Json;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using VoidRewardParser.Entities;

namespace VoidRewardParser.Logic
{
    public class MainViewModel : INotifyPropertyChanged
    {
        DispatcherTimer _parseTimer;
        DispatcherTimer _updatePlatPriceTimer;
        private List<DisplayPrime> _primeItems = new List<DisplayPrime>();
        private bool _warframeNotDetected;
        private bool showAllPrimes;
        private DateTime _lastMissionComplete;

        public DelegateCommand LoadCommand { get; set; }

        public List<DisplayPrime> PrimeItems
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

        public bool ShowAllPrimes
        {
            get
            {
                return showAllPrimes;
            }
            set
            {
                if (showAllPrimes == value) return;
                showAllPrimes = value;
                if (showAllPrimes)
                {
                    foreach(var primeItem in PrimeItems)
                    {
                        primeItem.Visible = true;
                    }
                }
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

            _updatePlatPriceTimer = new DispatcherTimer();
            _updatePlatPriceTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _updatePlatPriceTimer.Tick += _updatePlatPriceTimer_Tick;
            _updatePlatPriceTimer.Start();

            LoadCommand = new DelegateCommand(LoadData);
        }

        private async void LoadData()
        {
            var primeData = await PrimeData.GetInstance();
            foreach(var primeItem in primeData.Primes)
            {
                PrimeItems.Add(new DisplayPrime() { Data = primeData.GetDataForItem(primeItem), Prime = primeItem });
            }
        }

        private bool VisibleFilter(object item)
        {
            var prime = item as DisplayPrime;
            return prime.Visible;
        }

        private async void _parseTimer_Tick(object sender, object e)
        {
            if (Warframe.WarframeIsRunning())
            {
                var text = await ScreenCapture.ParseTextAsync();
                
                var hiddenPrimes = new List<DisplayPrime>();
                foreach (var p in PrimeItems)
                {
                    if (text.Contains(LocalizationManager.Localize(p.Prime.Name)))
                    {
                        p.Visible = true;
                    }
                    else
                    {
                        hiddenPrimes.Add(p);
                    }
                }

                if (!ShowAllPrimes)
                {
                    if (hiddenPrimes.Count < PrimeItems.Count)
                    {
                        //Only hide if we see at least one prime (let the old list persist until we need to refresh)
                        foreach (var p in hiddenPrimes) { p.Visible = false; }
                    }
                }

                if(text.Contains(LocalizationManager.MissionSuccess) && _lastMissionComplete.AddMinutes(1) > DateTime.Now && 
                    PrimeItems.Count - hiddenPrimes.Count == 1)
                {
                    //Auto-record the selected reward if we detect a prime on the mission complete screen
                    _lastMissionComplete = DateTime.MinValue;
                    await PrimeItems.FirstOrDefault(p => p.Visible)?.AddCommand?.Execute();
                }

                if (text.Contains(LocalizationManager.MissionComplete))
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

        private async void _updatePlatPriceTimer_Tick(object sender, object e)
        {
            // TODO: cache plat prices for some time so that warframe.market doesn't get overly bombed with requests
            foreach(var p in PrimeItems)
            {
                if(p.Visible)
                {
                    if (p.Prime.PlatinumPrice == null)
                    {
                        p.Prime.PlatinumPrice = "...";
                        p.OnNotifyPropertyChanged("Prime");
                    }

                    var textInfo = new CultureInfo("en-US", false).TextInfo;

                    var partName = textInfo.ToTitleCase(p.Prime.Name.ToLower());

                    var removeBPSuffixPhrases = new string[]
                    {
                        "Blade", "Gauntlet", "Link", "Receiver", "Handle", "Ornament", "Neuroptics", "Chassis", "Systems", "Stock", "Barrel"
                    };

                    var removeBPSuffix = false;

                    foreach(var phrase in removeBPSuffixPhrases)
                    {
                        if(partName.EndsWith(phrase + " Blueprint"))
                        {
                            removeBPSuffix = true;
                        }
                    }

                    if(removeBPSuffix) partName = partName.Replace(" Blueprint", "");

                    // Since Warframe.Market is still using the term Helmet instead of the new one, TODO: this might change
                    partName = partName.Replace("Neuroptics", "Helmet");

                    using (var client = new WebClient())
                    {
                        string baseUrl = "https://warframe.market/api/get_orders/Blueprint/";

                        var uri = new Uri(baseUrl + partName);

                        client.DownloadStringCompleted += (_, ev) =>
                        {
                            dynamic result = JsonConvert.DeserializeObject(ev.Result);

                            // when the server responds anything that is not 200 (HTTP OK) don't bother doing something else
                            if(result.code.Value > 200)
                            {
                                Console.WriteLine("Error with {0}, Status Code: {1}", partName, result.code.Value);
                                p.Prime.PlatinumPrice = "...";
                                p.OnNotifyPropertyChanged("Prime");
                                return;
                            }

                            var smallestPrice = long.MaxValue;

                            foreach(var sellOrder in result.response.sell)
                            {
                                // only users who're online are interesting usually
                                if(sellOrder.online_status.Value || sellOrder.online_ingame.Value)
                                {
                                    if(sellOrder.price < smallestPrice)
                                    {
                                        smallestPrice = sellOrder.price.Value;
                                    }
                                }
                            }

                            p.Prime.PlatinumPrice = String.Format("{0}p", smallestPrice);
                            p.OnNotifyPropertyChanged("Prime");
                        };

                        await client.DownloadStringTaskAsync(uri);
                    }
                }
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

        public async void Close()
        {
            (await PrimeData.GetInstance()).SaveToFile();
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
