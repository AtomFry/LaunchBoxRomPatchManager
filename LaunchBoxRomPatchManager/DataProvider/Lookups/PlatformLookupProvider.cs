using LaunchBoxRomPatchManager.Model;
using System.Collections.Generic;
using System.Linq;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace LaunchBoxRomPatchManager.DataProvider.Lookups
{
    public class PlatformLookupProvider : ILookupProvider<IPlatform>
    {
        public PlatformLookupProvider()
        {
        }

        public IEnumerable<LookupItem> GetLookup()
        {
            return PluginHelper.DataManager.GetAllPlatforms()
                .Select(e => new LookupItem
                {
                    Id = e.Name,
                    DisplayValue = e.Name
                })
                .OrderBy(l => l.DisplayValue)
                .ToList();
        }
    }
}
