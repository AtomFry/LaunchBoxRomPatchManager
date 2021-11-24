using LaunchBoxRomPatchManager.DataProvider.Lookups;
using LaunchBoxRomPatchManager.Event;
using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.Model;
using LaunchBoxRomPatchManager.ViewModel.ViewModelBase;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LaunchBoxRomPatchManager.ViewModel
{
    public class RomPatcherNavigationViewModel : ViewModelBase.ViewModelBase
    {
        private RomPatcherLookupProvider _romPatcherLookupProvider;
        private IEventAggregator _eventAggregator;

        public RomPatcherNavigationViewModel()
        {
            _eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            _romPatcherLookupProvider = new RomPatcherLookupProvider();

            RomPatchers = new ObservableCollection<RomPatcherNavigationItemViewModel>();

            _eventAggregator.GetEvent<AfterRomPatcherSavedEvent>().Subscribe(AfterRomPatcherSaved);
            _eventAggregator.GetEvent<AfterRomPatcherDeletedEvent>().Subscribe(AfterRomPatcherDeleted);
        }

        public void Load()
        {
            RomPatchers.Clear();

            IEnumerable<LookupItem> lookup = _romPatcherLookupProvider.GetLookup();

            foreach(LookupItem item in lookup)
            {
                RomPatchers.Add(new RomPatcherNavigationItemViewModel(item.Id, item.DisplayValue));
            }
        }

        public ObservableCollection<RomPatcherNavigationItemViewModel> RomPatchers { get; }

        private void AfterRomPatcherDeleted(string romPatcherId)
        {
            RomPatcherNavigationItemViewModel romPatcher = RomPatchers.SingleOrDefault(rp => rp.Id == romPatcherId);
            if (romPatcher != null)
            {
                RomPatchers.Remove(romPatcher);
            }
        }

        private void AfterRomPatcherSaved(AfterRomPatcherSavedEventArgs obj)
        {
            RomPatcherNavigationItemViewModel lookupItem = RomPatchers.SingleOrDefault(l => l.Id == obj.Id);
            if (lookupItem == null)
            {
                RomPatchers.Add(new RomPatcherNavigationItemViewModel(obj.Id, obj.DisplayValue));
            }
            else
            {
                lookupItem.DisplayValue = obj.DisplayValue;
            }
        }

    }
}