using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using VoidRewardParser.Logic;
using Windows.Globalization;
using Windows.Media.Ocr;
using Windows.Storage;

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
        }
    }
}
