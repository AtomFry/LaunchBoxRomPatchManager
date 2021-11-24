using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LaunchBoxRomPatchManager.Helpers
{
    public sealed class DirectoryInfoHelper
    {
        private static readonly DirectoryInfoHelper instance = new DirectoryInfoHelper();

        // path to the big box application directory
        private string applicationPath;
        public string ApplicationPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(applicationPath))
                {
                    applicationPath = Directory.GetCurrentDirectory();
                }

                return applicationPath;
            }
        }

        // path to launchbox images folder 
        private string launchboxImagesPath;
        public string LaunchboxImagesPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(launchboxImagesPath))
                {
                    launchboxImagesPath = $"{ApplicationPath}\\Images";
                }

                return launchboxImagesPath;
            }
        }

        private string launchboxImagesPlatformsPath;
        public string LaunchboxImagesPlatformsPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(launchboxImagesPlatformsPath))
                {
                    launchboxImagesPlatformsPath = $"{LaunchboxImagesPath}\\Platforms";
                }
                return launchboxImagesPlatformsPath;
            }
        }

        private string pluginFolder;
        public string PluginFolder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(pluginFolder))
                {
                    pluginFolder = $"{ApplicationPath}\\Plugins\\LaunchBoxRomPatchManager";
                }
                return pluginFolder;
            }
        }

        private string logFile;
        public string LogFile
        {
            get
            {
                if (string.IsNullOrWhiteSpace(logFile))
                {
                    logFile = $"{PluginFolder}\\log.txt";
                }
                return logFile;
            }
        }

        private string romPatcherDataFilePath;
        public string RomPatcherDataFilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(romPatcherDataFilePath))
                {
                    romPatcherDataFilePath = $"{PluginFolder}\\RomPatchers.json";
                }
                return romPatcherDataFilePath;
            }
        }

        private string romPatcherDataFileBackupPath;
        public string RomPatcherDataFileBackupPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(romPatcherDataFileBackupPath))
                {
                    romPatcherDataFileBackupPath = $"{PluginFolder}\\DataBackup";
                }
                return romPatcherDataFileBackupPath;
            }
        }

        private string platformDataFilePath;
        public string PlatformDataFilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(platformDataFilePath))
                {
                    platformDataFilePath = $"{PluginFolder}\\Platforms.json";
                }
                return platformDataFilePath;
            }
        }

        private string gameDataFilePath;
        public string GameDataFilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(gameDataFilePath))
                {
                    gameDataFilePath = $"{PluginFolder}\\Games.json";
                }
                return gameDataFilePath;
            }
        }




        private string bigBoxSettingsFile;
        public string BigBoxSettingsFile
        {
            get
            {
                if (string.IsNullOrWhiteSpace(BigBoxSettingsFile))
                {
                    bigBoxSettingsFile = $"{ApplicationPath}\\Data\\BigBoxSettings.xml";
                }
                return bigBoxSettingsFile;
            }
        }

        private string launchBoxSettingsFile;
        public string LaunchBoxSettingsFile
        {
            get
            {
                if (string.IsNullOrWhiteSpace(launchBoxSettingsFile))
                {
                    launchBoxSettingsFile = $"{ApplicationPath}\\Data\\Settings.xml";
                }
                return launchBoxSettingsFile;
            }
        }


        public static void CreateFolders()
        {
            CreateFolder(DirectoryInfoHelper.Instance.PluginFolder);
        }

        public static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string ResourceFolder
        {
            get
            {
                return "pack://application:,,,/LaunchBoxRomPatchManager;component/resources";
            }
        }

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static DirectoryInfoHelper()
        {
        }

        private DirectoryInfoHelper()
        {

        }

        public static DirectoryInfoHelper Instance
        {
            get
            {
                return instance;
            }
        }

        public static void CreateFileIfNotExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                CreateDirectoryIfNotExists(filePath);

                File.WriteAllText(filePath, "");
            }
        }

        public static void CreateDirectoryIfNotExists(string filePath)
        {
            string folderPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }
    }
}
