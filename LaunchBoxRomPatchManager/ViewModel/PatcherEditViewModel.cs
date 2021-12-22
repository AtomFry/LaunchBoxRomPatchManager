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
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace LaunchBoxRomPatchManager.ViewModel
{
    public class PatcherEditViewModel : ViewModelBase.ViewModelBase
    {
        private PatcherDataProvider patcherDataProvider;
        private IEventAggregator eventAggregator;
        private Patcher patcher;
        private string selectedRemainingPlatform;
        private string selectedPatcherPlatform;

        public ICommand AddPlatformCommand { get; }
        public ICommand RemovePlatformCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand BrowsePatcherPathCommand { get; }

        public ObservableCollection<string> PatcherPlatforms { get; set; }
        public ObservableCollection<string> RemainingPlatforms { get; set; }

        public PatcherEditViewModel(Patcher _patcher)
        {
            patcher = _patcher;
            PatcherPlatforms = new ObservableCollection<string>();
            RemainingPlatforms = new ObservableCollection<string>();

            InitializePlatforms();

            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            patcherDataProvider = new PatcherDataProvider();

            AddPlatformCommand = new DelegateCommand(OnAddPatcherPlatformExecute);
            RemovePlatformCommand = new DelegateCommand(OnRemovePatcherPlatformExecute);
            SaveCommand = new DelegateCommand(OnSaveExecuteAsync, OnSaveCanExecute);
            CloseCommand = new DelegateCommand(OnCloseExecute);
            BrowsePatcherPathCommand = new DelegateCommand(OnBrowsePatcherPathExecute);
        }

        private void InitializePlatforms()
        {
            IPlatform[] allPlatforms = PluginHelper.DataManager.GetAllPlatforms();

            foreach(IPlatform platform in allPlatforms)
            {
                if (patcher.Platforms.Contains(platform.Name))
                {
                    PatcherPlatforms.Add(platform.Name);
                }
                else
                {
                    RemainingPlatforms.Add(platform.Name);
                }
            }

            PatcherPlatforms.CollectionChanged += PatcherPlatforms_CollectionChanged;
        }

        private void OnRemovePatcherPlatformExecute()
        {
            if(SelectedPatcherPlatform != null)
            {
                PatcherPlatforms.Remove(SelectedPatcherPlatform);
                OnPropertyChanged("PatcherPlatforms");

                // unselect the patcher platform 
                SelectedPatcherPlatform = null;
                OnPropertyChanged("SelectedPatcherPlatform");
            }
        }

        private void OnAddPatcherPlatformExecute()
        {
            if(SelectedRemainingPlatform != null)
            {
                PatcherPlatforms.Add(SelectedRemainingPlatform);
                OnPropertyChanged("PatcherPlatforms");

                // unselect the remaining patcher platform 
                SelectedRemainingPlatform = null;
                OnPropertyChanged("SelectedRemainingPatcherPlatform");
            }
        }

        private void PatcherPlatforms_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            patcher.Platforms.Clear();
            foreach (string patcherPlatform in PatcherPlatforms)
            {
                patcher.Platforms.Add(patcherPlatform);
            }
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

        public string SelectedPatcherPlatform
        {
            get { return selectedPatcherPlatform; }
            set
            {
                selectedPatcherPlatform = value;
                OnPropertyChanged("SelectedPatcherPlatform");
                InvalidateCommands();
            }
        }

        public string SelectedRemainingPlatform
        {
            get { return selectedRemainingPlatform; }
            set
            {
                selectedRemainingPlatform = value;
                OnPropertyChanged("SelectedRemainingPlatform");
                InvalidateCommands();
            }
        }

        private void InvalidateCommands()
        {
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }
}