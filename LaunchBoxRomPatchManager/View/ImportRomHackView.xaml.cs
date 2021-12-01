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
        private ImportRomHackViewModel importRomHackViewModel;
        private EventAggregator eventAggregator;

        public ImportRomHackView(ImportRomHackViewModel _importRomHackViewModel)
        {
            InitializeComponent();

            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            eventAggregator.GetEvent<ImportRomHackCancel>().Subscribe(OnCancel);

            importRomHackViewModel = _importRomHackViewModel;
            DataContext = importRomHackViewModel;
            Loaded += ImportRomHackView_Loaded;
        }

        private void OnCancel()
        {
            Close();
        }

        private void ImportRomHackView_Loaded(object sender, RoutedEventArgs e)
        {
            importRomHackViewModel.LoadAsync();
        }
    }
}
