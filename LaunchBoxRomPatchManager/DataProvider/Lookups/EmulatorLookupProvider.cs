using LaunchBoxRomPatchManager.Model;
using System.Collections.Generic;
using System.Linq;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace LaunchBoxRomPatchManager.DataProvider.Lookups
{
    public class EmulatorLookupProvider : ILookupProvider<IEmulator>
    {
        public EmulatorLookupProvider()
        {
        }

        public IEnumerable<LookupItem> GetLookup()
        {
            return PluginHelper.DataManager.GetAllEmulators()
                .Select(e => new LookupItem
                {
                    Id = e.Id,
                    DisplayValue = e.Title
                })
                .OrderBy(l => l.DisplayValue)
                .ToList();
        }
    }
}
