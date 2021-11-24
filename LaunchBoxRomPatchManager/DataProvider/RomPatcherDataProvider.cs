using LaunchBoxRomPatchManager.DataAccess;
using LaunchBoxRomPatchManager.Model;
using System.Collections.Generic;

namespace LaunchBoxRomPatchManager.DataProvider
{
    public class RomPatcherDataProvider
    {
        public RomPatcherDataProvider()
        {
        }

        public RomPatcher GetRomPatcherById(string romPatcherId)
        {
            return RomPatcherDataService.Instance.GetRomPatcherById(romPatcherId);
        }

        public void SaveRomPatcher(RomPatcher romPatcher)
        {
            RomPatcherDataService.Instance.SaveRomPatcher(romPatcher);
        }

        public void DeleteRomPatcher(string romPatcherId)
        {
            RomPatcherDataService.Instance.DeleteRomPatcher(romPatcherId);
        }

        public IEnumerable<RomPatcher> GetAllRomPatchers()
        {
            return RomPatcherDataService.Instance.GetAllRomPatchers();
        }
    }
}
