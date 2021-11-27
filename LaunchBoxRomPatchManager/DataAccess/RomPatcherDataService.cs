using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LaunchBoxRomPatchManager.DataAccess
{
    [Obsolete]
    public sealed class RomPatcherDataService
    {
        private string StorageFile = DirectoryInfoHelper.Instance.RomPatcherDataFilePath;

        public RomPatcher GetRomPatcherById(string id)
        {
            List<RomPatcher> romPatchers = ReadFromFile();
            return romPatchers.Single(f => f.Id == id);
        }

        public void SaveRomPatcher(RomPatcher romPatcher)
        {
            if (string.IsNullOrWhiteSpace(romPatcher.Id))
            {
                InsertRomPatcher(romPatcher);
            }
            else
            {
                UpdateRomPatcher(romPatcher);
            }
        }

        public void DeleteRomPatcher(string romPatcherId)
        {
            List<RomPatcher> romPatchers = ReadFromFile();
            RomPatcher existing = romPatchers.Single(f => f.Id == romPatcherId);
            romPatchers.Remove(existing);
            SaveToFile(romPatchers);
        }

        private void UpdateRomPatcher(RomPatcher romPatcher)
        {
            List<RomPatcher> romPatchers = ReadFromFile();
            RomPatcher existing = romPatchers.Single(f => f.Id == romPatcher.Id);
            int indexOfExisting = romPatchers.IndexOf(existing);
            romPatchers.Insert(indexOfExisting, romPatcher);
            romPatchers.Remove(existing);
            SaveToFile(romPatchers);
        }

        private void InsertRomPatcher(RomPatcher romPatcher)
        {
            List<RomPatcher> romPatchers = ReadFromFile();
            romPatcher.Id = Guid.NewGuid().ToString();
            romPatchers.Add(romPatcher);
            SaveToFile(romPatchers);
        }

        public IEnumerable<RomPatcher> GetAllRomPatchers()
        {
            return ReadFromFile();
        }

        private void SaveToFile(List<RomPatcher> romPatcherList)
        {
            BackupDataFile();
            string json = JsonConvert.SerializeObject(romPatcherList, Formatting.Indented);
            File.WriteAllText(StorageFile, json);
        }

        private List<RomPatcher> ReadFromFile()
        {
            if (!File.Exists(StorageFile))
            {
                return new List<RomPatcher>
                {
                    new RomPatcher{Id=Guid.NewGuid().ToString(), Name="FLIPS", FilePath="", CommandLine=""},
                    new RomPatcher{Id=Guid.NewGuid().ToString(), Name="PPF-PDX", FilePath="", CommandLine=""}
                };
            }

            string json = File.ReadAllText(StorageFile);
            return JsonConvert.DeserializeObject<List<RomPatcher>>(json);
        }

        private void BackupDataFile()
        {
            // copy the file to backup folder before writing
            if (File.Exists(StorageFile))
            {
                string currentTimeString = DateTime.Now.ToString("yyyyMMdd_H_mm_ss");
                string newFilePath = $"{DirectoryInfoHelper.Instance.RomPatcherDataFileBackupPath}\\RomPatchers_{currentTimeString}.json";
                File.Copy(StorageFile, newFilePath);
            }
        }

        #region singleton implementation 
        public static RomPatcherDataService Instance
        {
            get
            {
                return instance;
            }
        }

        private static readonly RomPatcherDataService instance = new RomPatcherDataService();

        static RomPatcherDataService()
        {
        }

        private RomPatcherDataService()
        {

        }
        #endregion
    }
}
