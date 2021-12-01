using LaunchBoxRomPatchManager.DataProvider;
using LaunchBoxRomPatchManager.Event;
using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.Model;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
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
        public ObservableCollection<PatcherPlatform> PatcherPlatforms { get; set; }
        private PatcherPlatform selectedPatcherPlatform;
        public PatcherPlatform SelectedPatcherPlatform
        {
            get { return selectedPatcherPlatform; }
            set
            {
                selectedPatcherPlatform = value;
                OnPropertyChanged("SelectedPatcherPlatform");

                InvalidateCommands();
            }
        }
        public ICommand AddPlatformCommand { get; }
        public ICommand RemovePlatformCommand { get; }



        public PatcherEditViewModel(Patcher _patcher)
        {
            patcher = _patcher;
            PatcherPlatforms = new ObservableCollection<PatcherPlatform>();
            foreach(PatcherPlatform patcherPlatform in patcher.Platforms)
            {
                PatcherPlatforms.Add(patcherPlatform);
            }
            PatcherPlatforms.CollectionChanged += PatcherPlatforms_CollectionChanged;

            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            patcherDataProvider = new PatcherDataProvider();

            SaveCommand = new DelegateCommand(OnSaveExecuteAsync, OnSaveCanExecute);
            CloseCommand = new DelegateCommand(OnCloseExecute);
            BrowsePatcherPathCommand = new DelegateCommand(OnBrowsePatcherPathExecute);

            AddPlatformCommand = new DelegateCommand(OnAddPatcherPlatformExecute);
            RemovePlatformCommand = new DelegateCommand(OnRemovePatcherPlatformExecute, OnRemovePatcherPlatformCanExecute);


        }

        private void OnRemovePatcherPlatformExecute()
        {
            PatcherPlatforms.Remove(SelectedPatcherPlatform);
            SelectedPatcherPlatform = null;
            OnPropertyChanged("SelectedPatcherPlatform");
            OnPropertyChanged("PatcherPlatforms");

            InvalidateCommands();
        }

        private bool OnRemovePatcherPlatformCanExecute()
        {
            return SelectedPatcherPlatform != null;
        }

        private void OnAddPatcherPlatformExecute()
        {
            SelectedPatcherPlatform = new PatcherPlatform();
            PatcherPlatforms.Add(SelectedPatcherPlatform);

            OnPropertyChanged("SelectedPatcherPlatform");
            OnPropertyChanged("PatcherPlatforms");

            InvalidateCommands();
        }

        private void PatcherPlatforms_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            patcher.Platforms.Clear();
            foreach(PatcherPlatform patcherPlatform in PatcherPlatforms)
            {
                patcher.Platforms.Add(patcherPlatform);
            }

            InvalidateCommands();
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
            await patcherDataProvider.SavePatcherAsync(patcher);
            eventAggregator.GetEvent<PatcherSaved>().Publish(patcher.Id);
            eventAggregator.GetEvent<PatcherEditClose>().Publish();
        }

        private bool OnSaveCanExecute()
        {
            // todo: check if it's safe to save
            return true;
        }

        public string PatcherId
        {
            get { return patcher.Id; }
            set
            {
                patcher.Id = value;
                OnPropertyChanged("PatcherId");
            }
        }

        public string PatcherName
        {
            get { return patcher.Name; }
            set
            {
                patcher.Name = value;
                OnPropertyChanged("PatcherName");
            }
        }

        public string PatcherPath
        {
            get { return patcher.Path; }
            set
            {
                patcher.Path = value;
                OnPropertyChanged("PatcherPath");
            }
        }

        public string PatcherCommandLine
        {
            get { return patcher.CommandLine; }
            set
            {
                patcher.CommandLine = value;
                OnPropertyChanged("PatcherCommandLine");
            }
        }

        private void InvalidateCommands()
        {
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)RemovePlatformCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)AddPlatformCommand).RaiseCanExecuteChanged();            
        }
    }
}