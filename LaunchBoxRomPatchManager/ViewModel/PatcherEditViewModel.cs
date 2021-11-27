using LaunchBoxRomPatchManager.DataProvider;
using LaunchBoxRomPatchManager.Event;
using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.Model;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using System;
using System.Windows.Input;

namespace LaunchBoxRomPatchManager.ViewModel
{
    public class PatcherEditViewModel : ViewModelBase.ViewModelBase
    {
        private PatcherDataProvider patcherDataProvider;
        private IEventAggregator eventAggregator;

        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand BrowsePatcherPathCommand { get; }

        private Patcher patcher;
        public Patcher Patcher 
        {
            get { return patcher; }
            set
            {
                patcher = value;
                OnPropertyChanged("Patcher");
            }
        }

        public PatcherEditViewModel(Patcher _patcher)
        {
            Patcher = _patcher;

            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            patcherDataProvider = new PatcherDataProvider();

            eventAggregator.GetEvent<PatcherSelected>().Subscribe(OnPatcherSelectedAsync);

            SaveCommand = new DelegateCommand(OnSaveExecuteAsync, OnSaveCanExecute);
            CloseCommand = new DelegateCommand(OnCloseExecute);
            BrowsePatcherPathCommand = new DelegateCommand(OnBrowsePatcherPathExecute);
        }

        private void OnBrowsePatcherPathExecute()
        {
            OpenFileDialog openFileDialog;
            openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            openFileDialog.Title = "Select patcher executable";
            if(openFileDialog.ShowDialog() == true)
            {
                PatcherPath = openFileDialog.FileName;
            }
        }

        private void OnCloseExecute()
        {
            eventAggregator.GetEvent<PatcherEditClose>().Publish();
        }

        private async void OnSaveExecuteAsync()
        {
            await patcherDataProvider.SavePatcherAsync(Patcher);
            eventAggregator.GetEvent<PatcherSaved>().Publish(Patcher.Id);
            eventAggregator.GetEvent<PatcherEditClose>().Publish();
        }

        private bool OnSaveCanExecute()
        {
            // todo: check if it's safe to save
            return true;
        }

        private async void OnPatcherSelectedAsync(string patcherId)
        {
            Patcher = (!string.IsNullOrWhiteSpace(patcherId))
                ? await patcherDataProvider.GetPatcherByIdAsync(patcherId)
                : new Patcher();

            /*
            PatcherId = Patcher.Id;
            PatcherName = Patcher.Name;
            PatcherPath = Patcher.Path;
            PatcherCommandLine = Patcher.CommandLine;
            */

            // todo: InitializeRomPatcher(romPatcher);

            // todo: LoadPlatformLookup();

            // todo: LoadRomPatcherPlatforms(romPatcher.Platforms);

            // todo: InvalidateCommands();
        }

        public string PatcherId
        {
            get { return Patcher.Id; }
            set
            {
                Patcher.Id = value;
                OnPropertyChanged("PatcherId");
            }
        }

        public string PatcherName
        {
            get { return Patcher.Name; }
            set
            {
                Patcher.Name = value;
                OnPropertyChanged("PatcherName");
            }
        }

        public string PatcherPath
        {
            get { return Patcher.Path; }
            set
            {
                Patcher.Path = value;
                OnPropertyChanged("PatcherPath");
            }
        }

        public string PatcherCommandLine
        {
            get { return Patcher.CommandLine; }
            set
            {
                Patcher.CommandLine = value;
                OnPropertyChanged("PatcherCommandLine");
            }
        }
    }
}