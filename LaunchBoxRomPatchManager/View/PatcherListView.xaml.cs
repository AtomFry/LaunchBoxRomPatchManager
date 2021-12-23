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
    /// Interaction logic for PatcherListView.xaml
    /// </summary>
    public partial class PatcherListView : Window
    {
        readonly PatcherListViewModel patcherListViewModel;
        readonly EventAggregator eventAggregator;

        public PatcherListView()
        {
            InitializeComponent();

            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            eventAggregator.GetEvent<PatcherListClose>().Subscribe(OnPatcherListClose);

            patcherListViewModel = new PatcherListViewModel();
            DataContext = patcherListViewModel;
            Loaded += PatcherListView_Loaded;
            PreviewKeyDown += PatcherListView_PreviewKeyDown;
        }

        private void PatcherListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void OnPatcherListClose()
        {
            Close();
        }

        private async void PatcherListView_Loaded(object sender, RoutedEventArgs e)
        {
            await patcherListViewModel.LoadAsync();
        }
    }
}