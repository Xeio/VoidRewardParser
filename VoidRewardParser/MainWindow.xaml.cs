using System;
using System.Windows;
using VoidRewardParser.Logic;

namespace VoidRewardParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;
            ViewModel.MissionComplete += ViewModel_MissionComplete;
        }

        private void ViewModel_MissionComplete(object sender, EventArgs e)
        {
            Activate();
            Focus();
        }
    }
}
