using LaunchBoxRomPatchManager.ViewModel;
using System.Windows;

namespace LaunchBoxRomPatchManager.View
{
    public partial class RomPatcherMainWindow : Window
    {
        RomPatcherMainViewModel _romPatcherMainViewModel;

        public RomPatcherMainWindow()
        {
            InitializeComponent();

            _romPatcherMainViewModel = new RomPatcherMainViewModel();

            DataContext = _romPatcherMainViewModel;

            Loaded += RomPatcherMainWindow_Loaded;
        }

        private void RomPatcherMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _romPatcherMainViewModel.Load();
        }
    }
}
