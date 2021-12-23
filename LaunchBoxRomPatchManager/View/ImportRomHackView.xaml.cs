using LaunchBoxRomPatchManager.Event;
using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.ViewModel;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LaunchBoxRomPatchManager.View
{
    /// <summary>
    /// Interaction logic for ImportRomHackView.xaml
    /// </summary>
    public partial class ImportRomHackView : Window
    {
        private readonly ImportRomHackViewModel importRomHackViewModel;
        private readonly EventAggregator eventAggregator;

        public ImportRomHackView(ImportRomHackViewModel _importRomHackViewModel)
        {
            InitializeComponent();

            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            eventAggregator.GetEvent<ImportRomHackCancel>().Subscribe(OnCancel);

            importRomHackViewModel = _importRomHackViewModel;
            DataContext = importRomHackViewModel;
            Loaded += ImportRomHackView_Loaded;
            PreviewKeyDown += ImportRomHackView_PreviewKeyDown;
        }

        private void ImportRomHackView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void OnCancel()
        {
            Close();
        }

        private void ImportRomHackView_Loaded(object sender, RoutedEventArgs e)
        {
            importRomHackViewModel.LoadAsync();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Grid)
            {
                WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            }
        }
    }
}
