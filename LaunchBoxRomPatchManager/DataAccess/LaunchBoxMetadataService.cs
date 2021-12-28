using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LaunchBoxRomPatchManager.DataAccess
{
    public class LaunchBoxMetadataService
    {
        private string LaunchBoxMetadataFile = DirectoryInfoHelper.Instance.LaunchBoxMetadataFile;
        private string RomHackMetadataFile = DirectoryInfoHelper.Instance.PluginMetadataFile;

        public async Task<IEnumerable<MetadataGame>> GetRomHackMetadataAsync()
        {
            return await ReadFromFileAsync();
        }

        private async Task<List<MetadataGame>> ReadFromFileAsync()
        {
            List<MetadataGame> emptyList = new List<MetadataGame>();

            // if the LaunchBox metadata file doesn't exist just return an empty list 
            if (!File.Exists(LaunchBoxMetadataFile))
            {
                return emptyList;
            }

            try
            {
                if (!File.Exists(RomHackMetadataFile))
                {
                    // make sure the folder exists 
                    DirectoryInfoHelper.CreateFolders();

                    // try to create the RomHackMetadataFile from the LaunchBoxMetadataFile
                    CreateRomHackMetadataFromLaunchBoxMetadata(LaunchBoxMetadataFile, RomHackMetadataFile);
                }
                else if (File.GetLastWriteTime(LaunchBoxMetadataFile) > File.GetLastWriteTime(RomHackMetadataFile))
                {
                    // create/update the RomhackMetadataFile if the LaunchBoxMetadataFile is newer
                    CreateRomHackMetadataFromLaunchBoxMetadata(LaunchBoxMetadataFile, RomHackMetadataFile);
                }

                // read and deserialize the file
                return await Task.Run(() =>
                {
                    string json = File.ReadAllText(RomHackMetadataFile);
                    return JsonConvert.DeserializeObject<List<MetadataGame>>(json);
                });
            }
            catch(Exception ex)
            {
                LogHelper.LogException(ex, "Getting metadata");
            }

            // if an exception occurred or for whatever reason we got here, return an empty list
            return emptyList;
        }

        private static void CreateRomHackMetadataFromLaunchBoxMetadata(string filename, string outputFileName)
        {
            // Create an instance of the XmlSerializer
            XmlSerializer serializer = new XmlSerializer(typeof(LaunchBox));

            // Declare an object variable of the type to be deserialized.
            LaunchBox launchBox;

            using (Stream reader = new FileStream(filename, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                launchBox = (LaunchBox)serializer.Deserialize(reader);
            }

            IEnumerable<MetadataGame> romHackGames = launchBox.Games.Where(g =>
                string.IsNullOrWhiteSpace(g.ReleaseType) == false &&
                g.ReleaseType.ToLower().Contains("hack"));

            string json = JsonConvert.SerializeObject(romHackGames, Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(outputFileName, json);
        }

        #region singleton implementation 
        public static LaunchBoxMetadataService Instance
        {
            get
            {
                return instance;
            }
        }

        private static readonly LaunchBoxMetadataService instance = new LaunchBoxMetadataService();

        static LaunchBoxMetadataService()
        {
        }

        private LaunchBoxMetadataService()
        {
        }
        #endregion
    }
}
