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
            Topmost = true;
            Topmost = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ViewModel.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadCommand.Execute();
        }
    }
}
