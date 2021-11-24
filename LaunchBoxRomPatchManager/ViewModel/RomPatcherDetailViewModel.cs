﻿using LaunchBoxRomPatchManager.DataProvider;
using LaunchBoxRomPatchManager.DataProvider.Lookups;
using LaunchBoxRomPatchManager.Event;
using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.Model;
using LaunchBoxRomPatchManager.ModelWrapper;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace LaunchBoxRomPatchManager.ViewModel
{
    public class RomPatcherDetailViewModel : ViewModelBase.ViewModelBase
    {
        private RomPatcherWrapper _romPatcher;
        private RomPatcherDataProvider _romPatcherDataProvider;
        private PlatformLookupProvider _platformLookupProvider;
        private IEventAggregator _eventAggregator;

        public RomPatcherDetailViewModel()
        {
            _eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            _romPatcherDataProvider = new RomPatcherDataProvider();
            _platformLookupProvider = new PlatformLookupProvider();

            SaveCommand = new DelegateCommand(OnSaveExecute, OnSaveCanExecute);
            DeleteCommand = new DelegateCommand(OnDeleteExecute);

            AddPlatformCommand = new DelegateCommand(OnAddPlatformExecute);
            RemovePlatformCommand = new DelegateCommand(OnRemovePlatformExecute, OnRemotePlatformCanExecute);

            PlatformLookup = new ObservableCollection<LookupItem>();
            Platforms = new ObservableCollection<RomPatcherPlatformWrapper>();
        }

        public void Load(string romPatcherId)
        {
            RomPatcher romPatcher = (!string.IsNullOrWhiteSpace(romPatcherId))
                ? _romPatcherDataProvider.GetRomPatcherById(romPatcherId)
                : CreateNewRomPatcher();

            InitializeRomPatcher(romPatcher);

            LoadPlatformLookup();

            LoadRomPatcherPlatforms(romPatcher.Platforms);

            InvalidateCommands();
        }

        private void LoadRomPatcherPlatforms(List<RomPatcherPlatform> platforms)
        {
            foreach (var wrapper in Platforms)
            {
                wrapper.PropertyChanged -= RomPatcherPlatformWrapper_PropertyChanged;
            }
            Platforms.Clear();
            foreach (var romPatcherPlatform in platforms)
            {
                var wrapper = new RomPatcherPlatformWrapper(romPatcherPlatform);
                Platforms.Add(wrapper);
                wrapper.PropertyChanged += RomPatcherPlatformWrapper_PropertyChanged;
            }

        }

        private void RomPatcherPlatformWrapper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {            
            InvalidateCommands();
        }

        private void InitializeRomPatcher(RomPatcher romPatcher)
        {
            RomPatcher = new RomPatcherWrapper(romPatcher);
            RomPatcher.PropertyChanged += RomPatcher_PropertyChanged;
        }

        private void LoadPlatformLookup()
        {
            PlatformLookup.Clear();
            PlatformLookup.Add(new NullLookupItem());
            IEnumerable<LookupItem> lookup = _platformLookupProvider.GetLookup();
            foreach (LookupItem lookupItem in lookup)
            {
                PlatformLookup.Add(lookupItem);
            }
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
                InvalidateCommands();
            }
        }

        private void OnSaveExecute()
        {
            // save the rom patcher
            _romPatcherDataProvider.SaveRomPatcher(RomPatcher.Model);

            // changes are saved so accept them to reset change tracking
            RomPatcher.AcceptChanges();

            foreach(var plat in Platforms)
            {
                plat.AcceptChanges();
            }

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
            return RomPatcher != null
                && RomPatcher.IsValid 
                && Platforms.All(p => !p.HasErrors)
                && 
                (
                    RomPatcher.Platforms.IsChanged || RomPatcher.IsChanged
                );
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

        private void OnRemovePlatformExecute()
        {
            SelectedRomPatcherPlatform.PropertyChanged -= RomPatcherPlatformWrapper_PropertyChanged;
            RomPatcher.Platforms.Remove(SelectedRomPatcherPlatform);
            RomPatcher.Model.Platforms.Remove(SelectedRomPatcherPlatform.Model);
            Platforms.Remove(SelectedRomPatcherPlatform);
            SelectedRomPatcherPlatform = null;
            InvalidateCommands();
        }

        private bool OnRemotePlatformCanExecute()
        {
            // enable remove platform button if one is selected
            return SelectedRomPatcherPlatform != null;
        }

        private void OnAddPlatformExecute()
        {
            var newPlatform = new RomPatcherPlatformWrapper(new RomPatcherPlatform());
            newPlatform.PropertyChanged += RomPatcherPlatformWrapper_PropertyChanged;
            Platforms.Add(newPlatform);
            RomPatcher.Model.Platforms.Add(newPlatform.Model);
            newPlatform.PlatformId = "";
        }

        private void InvalidateCommands()
        {
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)DeleteCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)AddPlatformCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)RemovePlatformCommand).RaiseCanExecuteChanged();
        }

        private RomPatcher CreateNewRomPatcher()
        {
            RomPatcher romPatcher = new RomPatcher();
            // romPatcher.Emulators = new System.Collections.Generic.List<RomPatcherEmulator>();
            // romPatcher.Platforms = new System.Collections.Generic.List<RomPatcherPlatform>();
            return romPatcher;
        }
    }
}
