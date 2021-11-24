using LaunchBoxRomPatchManager.Event;
using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.ViewModel.ViewModelBase;
using Prism.Commands;
using Prism.Events;
using System;
using System.Windows.Input;

namespace LaunchBoxRomPatchManager.ViewModel
{
    public class RomPatcherNavigationItemViewModel : ViewModelBase.ViewModelBase
    {
        private IEventAggregator _eventAggregator;
        private string _displayValue;

        public RomPatcherNavigationItemViewModel(string id, string displayValue)
        {
            _eventAggregator = EventAggregatorHelper.Instance.EventAggregator;

            Id = id;
            DisplayValue = displayValue;

            OpenRomPatcherDetailViewCommand = new DelegateCommand(OnOpenRomPatcherDetailView);
        }


        public string Id { get; }

        public string DisplayValue
        {
            get { return _displayValue; }
            set
            {
                _displayValue = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenRomPatcherDetailViewCommand { get; }
        private void OnOpenRomPatcherDetailView()
        {
            _eventAggregator.GetEvent<OpenRomPatcherDetailViewEvent>().Publish(Id);
        }
    }
}
