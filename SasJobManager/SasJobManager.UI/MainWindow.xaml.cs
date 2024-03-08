using Microsoft.Extensions.Configuration;
using SasJobManager.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace SasJobManager.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;
        private string _root;

 
        public MainWindow(MainViewModel viewModel)
        {
            //var appTheme = new VisualStudio2019Theme();
            //VisualStudio2019Palette.Palette.FontSize = 10;
            //StyleManager.ApplicationTheme = appTheme;
            
            _vm =viewModel;
            InitializeComponent();
            DataContext = _vm;
            Title = $"SAS Batch Job Launcher";
            Files = new List<string>();

            Loaded += MainWindow_Loaded;
        }
        public string Root
        {
            get { return _root; }
            set
            {
                _root = value;
            }
        }
        public List<string> Files { get; set; }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _vm.Load(Root,Files);
        }
    }
}
