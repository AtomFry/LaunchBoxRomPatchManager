using LaunchBoxRomPatchManager.DataProvider;
using LaunchBoxRomPatchManager.Event;
using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.Model;
using LaunchBoxRomPatchManager.View;
using Prism.Commands;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LaunchBoxRomPatchManager.ViewModel
{
    public class PatcherListViewModel : ViewModelBase.ViewModelBase
    {
        private PatcherDataProvider patcherDataProvider;
        private IEventAggregator eventAggregator;
        public ObservableCollection<Patcher> Patchers { get; }
        public ICommand EditCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CloseCommand { get; }

        private Patcher selectedPatcher;

        public Patcher SelectedPatcher
        {
            get { return selectedPatcher; }
            set
            {
                selectedPatcher = value;
                OnPropertyChanged();
                InvalidateCommands();
            }
        }

        private PatcherEditViewModel patcherEditViewModel;
        private PatcherEditView patcherEditView;


        public PatcherListViewModel()
        {
            Patchers = new ObservableCollection<Patcher>();

            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            patcherDataProvider = new PatcherDataProvider();

            EditCommand = new DelegateCommand(OnEditExecute, OnEditCanExecute);
            AddCommand = new DelegateCommand(OnAddExecute, OnAddCanExecute);
            DeleteCommand = new DelegateCommand(OnDeleteExecuteAsync, OnDeleteCanExecute);
            CloseCommand = new DelegateCommand(OnCloseExecute);

            eventAggregator.GetEvent<PatcherSaved>().Subscribe(OnPatcherSavedAsync);
            eventAggregator.GetEvent<PatcherEditClosing>().Subscribe(OnPatcherEditClosed);
        }

        private void OnPatcherEditClosed()
        {
            patcherEditView = null;
            patcherEditViewModel = null;
        }

        private async void OnPatcherSavedAsync(string patcherId)
        {
            await LoadAsync();

            Patcher patcher = Patchers.SingleOrDefault(p => p.Id == patcherId);
            if(patcher != null)
            {
                SelectedPatcher = patcher;
            }
        }

        private void OnCloseExecute()
        {
            eventAggregator.GetEvent<PatcherListClose>().Publish();
        }

        private async void OnDeleteExecuteAsync()
        {
            string patcherName = string.IsNullOrWhiteSpace(SelectedPatcher?.Name) ? "patcher" : SelectedPatcher.Name;

            MessageDialogResult messageDialogResult = MessageDialogHelper.ShowOKCancelDialog($"Delete {patcherName}?", "Delete patcher");
            if (messageDialogResult == MessageDialogResult.OK)
            {
                await patcherDataProvider.DeleteRomPatcher(SelectedPatcher.Id);
                await LoadAsync();
            }

            SelectedPatcher = null;
        }

        private bool OnDeleteCanExecute()
        {
            // if the window is closed (null) and a patcher is selected (not null)
            return (patcherEditView == null) && (SelectedPatcher != null);
        }

        private void OnAddExecute()
        {
            if (patcherEditView != null)
            {
                patcherEditView.Activate();
            }
            else
            {
                SelectedPatcher = null;
                patcherEditViewModel = new PatcherEditViewModel(new Patcher());
                patcherEditView = new PatcherEditView(patcherEditViewModel);
                patcherEditView.Show();
            }
        }

        private bool OnAddCanExecute()
        {
            // add is ok if the edit window is closed 
            return patcherEditView == null;
        }

        private void OnEditExecute()
        {
            if (patcherEditView != null)
            {
                patcherEditView.Activate();
            }
            else
            {
                patcherEditViewModel = new PatcherEditViewModel(SelectedPatcher);
                patcherEditView = new PatcherEditView(patcherEditViewModel);
                patcherEditView.Show();
            }
        }

        private bool OnEditCanExecute()
        {
            // if the window is closed (null) and a patcher is selected (not null)
            return (patcherEditView == null) && (SelectedPatcher != null);
        }

        public async Task LoadAsync()
        {
            Patchers.Clear();

            IEnumerable<Patcher> patchers = await patcherDataProvider.GetAllPatchersAsync();

            foreach (Patcher patcher in patchers)
            {
                Patchers.Add(patcher);
            }

            InvalidateCommands();
        }

        private void InvalidateCommands()
        {
            ((DelegateCommand)EditCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)DeleteCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)AddCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)CloseCommand).RaiseCanExecuteChanged();
        }
    }
}