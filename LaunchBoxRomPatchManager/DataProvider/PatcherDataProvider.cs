using LaunchBoxRomPatchManager.DataAccess;
using LaunchBoxRomPatchManager.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LaunchBoxRomPatchManager.DataProvider
{
    public class PatcherDataProvider
    {
        public PatcherDataProvider()
        {
        }

        public async Task<Patcher> GetPatcherByIdAsync(string patcherId)
        {
            return await PatcherDataService.Instance.GetPatcherByIdAsync(patcherId);
        }

        public async Task SavePatcherAsync(Patcher patcher)
        {
            await PatcherDataService.Instance.SavePatcherAsync(patcher);
        }

        public async Task DeleteRomPatcher(string patcherId)
        {
            await PatcherDataService.Instance.DeletePatcherAsync(patcherId);
        }

        public async Task<IEnumerable<Patcher>> GetAllPatchersAsync()
        {
            return await PatcherDataService.Instance.GetAllPatchersAsync();
        }
    }

}
