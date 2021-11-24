using LaunchBoxRomPatchManager.DataAccess;
using LaunchBoxRomPatchManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaunchBoxRomPatchManager.DataProvider.Lookups
{
    public class RomPatcherLookupProvider : ILookupProvider<RomPatcher>
    {
        public RomPatcherLookupProvider()
        {
        }

        public IEnumerable<LookupItem> GetLookup()
        {
            return RomPatcherDataService.Instance.GetAllRomPatchers()
                    .Select(rp => new LookupItem { Id = rp.Id, DisplayValue = rp.Name })
                    .OrderBy(ob => ob.DisplayValue)
                    .ToList();
        }
    }
}
