using LaunchBoxRomPatchManager.Model;
using System.Collections.Generic;

namespace LaunchBoxRomPatchManager.DataProvider.Lookups
{
    public interface ILookupProvider<T>
    {
        IEnumerable<LookupItem> GetLookup();
    }
}
