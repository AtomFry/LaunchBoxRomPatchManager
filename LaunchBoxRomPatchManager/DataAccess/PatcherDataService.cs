using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace LaunchBoxRomPatchManager.DataAccess
{
    public sealed class PatcherDataService
    {
        private readonly string StorageFile = DirectoryInfoHelper.Instance.RomPatcherDataFilePath;

        public async Task<Patcher> GetPatcherByIdAsync(string patcherId)
        {
            List<Patcher> patchers = await ReadFromFileAsync();
            return patchers.Single(f => f.Id == patcherId);
        }

        public async Task SavePatcherAsync(Patcher patcher)
        {
            if (string.IsNullOrWhiteSpace(patcher.Id))
            {
                await InsertPatcherAsync(patcher);
            }
            else
            {
                await UpdatePatcherAsync(patcher);
            }
        }

        public async Task DeletePatcherAsync(string patcherId)
        {
            List<Patcher> patchers = await ReadFromFileAsync();
            Patcher existing = patchers.Single(f => f.Id == patcherId);
            patchers.Remove(existing);
            await SaveToFileAsync(patchers);
        }

        private async Task UpdatePatcherAsync(Patcher patcher)
        {
            List<Patcher> patchers = await ReadFromFileAsync();
            Patcher existing = patchers.SingleOrDefault(f => f.Id == patcher.Id);
            int indexOfExisting = patchers.IndexOf(existing);
            patchers.Insert(indexOfExisting, patcher);
            patchers.Remove(existing);
            await SaveToFileAsync(patchers);
        }

        private async Task InsertPatcherAsync(Patcher patcher)
        {
            List<Patcher> patchers = await ReadFromFileAsync();
            patcher.Id = Guid.NewGuid().ToString();
            patchers.Add(patcher);
            await SaveToFileAsync(patchers);
        }

        public async Task<IEnumerable<Patcher>> GetAllPatchersAsync()
        {
            return await ReadFromFileAsync();
        }

        private async Task SaveToFileAsync(List<Patcher> patcherList)
        {
            await BackupDataFileAsync();

            await Task.Run(() =>
            {
                string json = JsonConvert.SerializeObject(patcherList, Formatting.Indented);
                File.WriteAllText(StorageFile, json);
            });
        }

        private async Task<List<Patcher>> ReadFromFileAsync()
        {
            // make sure the data file exists 
            if (!File.Exists(StorageFile))
            {
                // make sure the folders exist 
                DirectoryInfoHelper.CreateFolders();

                // create a sample list 
                List<Patcher> patchers = new List<Patcher>();
                Patcher patcher = new Patcher
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "FLIPS",
                    Path = $"LaunchBoxRomPatchManager\\Patchers\\Floating\\flips.exe",
                    CommandLine = "{patch} {rom}",
                    Platforms = PatcherPlatformHelper.GetDefaultIpsPlatforms()
                };
                patchers.Add(patcher);

                patcher = new Patcher
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "PPF-PDX",
                    Path = $"LaunchBoxRomPatchManager\\Patchers\\pdx-ppf3\\ppfbin\\applyppf\\w32\\ApplyPPF3.exe",
                    CommandLine = "a {rom} {patch}",
                    Platforms = PatcherPlatformHelper.GetDefaultPpfPlatforms()
                };
                patchers.Add(patcher);

                // save the file 
                await SaveToFileAsync(patchers);

                return patchers;
            }

            // read and deserialize the file
            return await Task.Run(() =>
            {
                string json = File.ReadAllText(StorageFile);
                return JsonConvert.DeserializeObject<List<Patcher>>(json);
            });
        }

        private async Task BackupDataFileAsync()
        {
            await Task.Run(() =>
            {
                // copy the file to backup folder before writing
                if (File.Exists(StorageFile))
                {
                    string currentTimeString = DateTime.Now.ToString("yyyyMMdd_H_mm_ss");
                    string newFilePath = $"{DirectoryInfoHelper.Instance.RomPatcherDataFileBackupPath}\\RomPatchers_{currentTimeString}.json";
                    DirectoryInfoHelper.CreateDirectoryIfNotExists(newFilePath);
                    File.Copy(StorageFile, newFilePath);
                }
            });
        }

        #region singleton implementation 
        public static PatcherDataService Instance
        {
            get
            {
                return instance;
            }
        }

        private static readonly PatcherDataService instance = new PatcherDataService();

        static PatcherDataService()
        {
        }

        private PatcherDataService()
        {
        }
        #endregion
    }
}
