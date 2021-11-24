using LaunchBoxRomPatchManager.Event;
using LaunchBoxRomPatchManager.Helpers;
using Prism.Commands;
using Prism.Events;
using System;
using System.Windows.Input;

namespace LaunchBoxRomPatchManager.ViewModel
{
    public class RomPatcherMainViewModel : ViewModelBase.ViewModelBase
    {
        private RomPatcherDetailViewModel _romPatcherDetailViewModel;

        private IEventAggregator _eventAggregator;

        public RomPatcherMainViewModel()
        {
            RomPatcherNavigationViewModel = new RomPatcherNavigationViewModel();
            RomPatcherDetailViewModel = new RomPatcherDetailViewModel();

            // register events
            _eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            _eventAggregator.GetEvent<OpenRomPatcherDetailViewEvent>().Subscribe(OnOpenRomPatcherDetailView);
            _eventAggregator.GetEvent<AfterRomPatcherDeletedEvent>().Subscribe(AfterRomPatcherDeleted);


            // setup the create rom patcher command 
            CreateNewRomPatcherCommand = new DelegateCommand(OnCreateNewRomPatcherExecute);
        }

        public void Load()
        {
            RomPatcherNavigationViewModel.Load();
        }

        public RomPatcherNavigationViewModel RomPatcherNavigationViewModel { get; }

        public RomPatcherDetailViewModel RomPatcherDetailViewModel
        {
            get { return _romPatcherDetailViewModel; }
            private set
            {
                _romPatcherDetailViewModel = value;
                OnPropertyChanged();
            }
        }

        private void OnOpenRomPatcherDetailView(string romPatcherId)
        {
            // prompt before discarding any changes
            if(RomPatcherDetailViewModel?.RomPatcher != null 
                && RomPatcherDetailViewModel.RomPatcher.IsChanged)
            {
                MessageDialogResult result = MessageDialogHelper.ShowOKCancelDialog("Unsaved changes will be lost, discard changes?", "Discard changes");
                if(result == MessageDialogResult.Cancel)
                {
                    return;
                }
            }

            RomPatcherDetailViewModel = new RomPatcherDetailViewModel();
            RomPatcherDetailViewModel.Load(romPatcherId);
        }

        public ICommand CreateNewRomPatcherCommand { get; }

        private void OnCreateNewRomPatcherExecute()
        {
            OnOpenRomPatcherDetailView(null);
        }

        private void AfterRomPatcherDeleted(string romPatcherId)
        {
            RomPatcherDetailViewModel = null;
        }
    }
}
