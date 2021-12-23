using LaunchBoxRomPatchManager.Event;
using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.ViewModel;
using Prism.Events;
using System.Windows;
using System.Windows.Input;

namespace LaunchBoxRomPatchManager.View
{

    public partial class PatcherEditView : Window
    {
        readonly EventAggregator eventAggregator;

        public PatcherEditView(PatcherEditViewModel patcherEditViewModel)
        {
            InitializeComponent();

            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            eventAggregator.GetEvent<PatcherEditClose>().Subscribe(OnPatcherEditClose);

            DataContext = patcherEditViewModel;

            Closing += PatcherEditView_Closing;
            PreviewKeyDown += PatcherEditView_PreviewKeyDown;
        }

        private void PatcherEditView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void PatcherEditView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            eventAggregator.GetEvent<PatcherEditClosing>().Publish();
        }

        private void OnPatcherEditClose()
        {
            eventAggregator.GetEvent<PatcherEditClosing>().Publish();
            Close();
        }
    }
}
