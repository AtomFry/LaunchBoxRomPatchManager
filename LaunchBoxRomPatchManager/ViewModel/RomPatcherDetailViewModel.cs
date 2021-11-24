using LaunchBoxRomPatchManager.DataProvider;
using LaunchBoxRomPatchManager.Event;
using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.Model;
using LaunchBoxRomPatchManager.ModelWrapper;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LaunchBoxRomPatchManager.ViewModel
{
    public class RomPatcherDetailViewModel : ViewModelBase.ViewModelBase
    {
        private RomPatcherWrapper _romPatcher;
        private RomPatcherDataProvider _romPatcherDataProvider;
        private IEventAggregator _eventAggregator;

        public RomPatcherDetailViewModel()
        {
            _eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            _romPatcherDataProvider = new RomPatcherDataProvider();

            SaveCommand = new DelegateCommand(OnSaveExecute, OnSaveCanExecute);
            DeleteCommand = new DelegateCommand(OnDeleteExecute);
        }


        public void Load(string romPatcherId)
        {
            RomPatcher romPatcher = (!string.IsNullOrWhiteSpace(romPatcherId))
                ? _romPatcherDataProvider.GetRomPatcherById(romPatcherId)
                : CreateNewRomPatcher();

            
            RomPatcher = new RomPatcherWrapper(romPatcher);

            RomPatcher.PropertyChanged += RomPatcher_PropertyChanged;

            InvalidateCommands();
        }


        private void RomPatcher_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(RomPatcher.IsChanged) || e.PropertyName == nameof(RomPatcher.IsValid))
            {
                InvalidateCommands();
            }
        }

        public RomPatcherWrapper RomPatcher
        {
            get { return _romPatcher; }
            private set
            {
                _romPatcher = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddPlatformCommand { get; }
        public ICommand RemovePlatformCommand { get; }
        public ObservableCollection<LookupItem> PlatformLookup { get; }
        public ObservableCollection<RomPatcherPlatformWrapper> Platforms { get; }

        private RomPatcherPlatformWrapper _selectedRomPatcherPlatform;
        public RomPatcherPlatformWrapper SelectedRomPatcherPlatform
        {
            get { return _selectedRomPatcherPlatform; }
            set
            {
                _selectedRomPatcherPlatform = value;
                OnPropertyChanged();
                ((DelegateCommand)RemovePlatformCommand).RaiseCanExecuteChanged();
            }
        }

        private void OnSaveExecute()
        {
            // save the rom patcher
            _romPatcherDataProvider.SaveRomPatcher(RomPatcher.Model);

            // changes are saved so accept them to reset change tracking
            RomPatcher.AcceptChanges();

            // publish an event to notify that a rom patcher has been saved 
            _eventAggregator.GetEvent<AfterRomPatcherSavedEvent>()
                .Publish(new AfterRomPatcherSavedEventArgs
                {
                    Id = RomPatcher.Id,
                    DisplayValue = RomPatcher.Name
                });

            // raise the can execute changed for the save button
            InvalidateCommands();
        }

        private bool OnSaveCanExecute()
        {
            return RomPatcher != null && RomPatcher.IsValid && RomPatcher.IsChanged;
        }

        private void OnDeleteExecute()
        {
            MessageDialogResult messageDialogResult = MessageDialogHelper.ShowOKCancelDialog($"Delete {RomPatcher.Name}?", "Confirm delete");

            if (messageDialogResult == MessageDialogResult.OK)
            {

                _romPatcherDataProvider.DeleteRomPatcher(RomPatcher.Id);

                // publish an event notifying that a rom patcher is deleted
                _eventAggregator.GetEvent<AfterRomPatcherDeletedEvent>().Publish(RomPatcher.Id);
            }
        }

        private void InvalidateCommands()
        {
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private RomPatcher CreateNewRomPatcher()
        {
            RomPatcher romPatcher = new RomPatcher();
            romPatcher.Emulators = new System.Collections.Generic.List<RomPatcherEmulator>();
            romPatcher.Platforms = new System.Collections.Generic.List<RomPatcherPlatform>();
            return romPatcher;
        }
    }
}
