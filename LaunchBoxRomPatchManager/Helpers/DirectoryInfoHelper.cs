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

        private string launchboxVideosPath;
        public string LaunchboxVideosPath
        {
            get
            {
                if(string.IsNullOrWhiteSpace(launchboxVideosPath))
                {
                    launchboxVideosPath = $"{ApplicationPath}\\Videos";
                }
                return launchboxVideosPath;
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

        private string launchboxGamesPath;
        public string LaunchboxGamesPath
        {
            get
            {
                if(string.IsNullOrWhiteSpace(launchboxGamesPath))
                {
                    launchboxGamesPath = $"{ApplicationPath}\\Games";
                }
                return launchboxGamesPath;
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

        private string romPatcherDataFolderPath;
        public string RomPatcherDataFolderPath
        {
            get
            {
                if(string.IsNullOrWhiteSpace(romPatcherDataFolderPath))
                {
                    romPatcherDataFolderPath = $"{PluginFolder}\\Data";
                }
                return romPatcherDataFolderPath;
            }
        }

        private string romPatcherDataFilePath;
        public string RomPatcherDataFilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(romPatcherDataFilePath))
                {
                    romPatcherDataFilePath = $"{RomPatcherDataFolderPath}\\RomPatchers.json";
                }
                return romPatcherDataFilePath;
            }
        }

        private string pluginTempFolder;
        public string PluginTempFolder
        {
            get
            {
                if(string.IsNullOrWhiteSpace(pluginTempFolder))
                {
                    pluginTempFolder = $"{PluginFolder}\\Temp";
                }
                return pluginTempFolder;
            }
        }

        private string romPatcherDataFileBackupPath;
        public string RomPatcherDataFileBackupPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(romPatcherDataFileBackupPath))
                {
                    romPatcherDataFileBackupPath = $"{RomPatcherDataFolderPath}\\DataBackup";
                }
                return romPatcherDataFileBackupPath;
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
            CreateFolder(Instance.PluginFolder);
            CreateFolder(Instance.RomPatcherDataFolderPath);
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

        public static string GetCleanFileName(string fileName)
        {
            char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

            string cleanFileName = fileName;

            if (!string.IsNullOrWhiteSpace(cleanFileName))
            {
                foreach (char invalidChar in InvalidFileNameChars)
                {
                    cleanFileName = cleanFileName.Replace(invalidChar, '_');
                }
                return cleanFileName;
            }
            return string.Empty;
        }

        public static void FixDirectoryAttributes(DirectoryInfo directory)
        {
            foreach (var subDir in directory.GetDirectories())
            {
                FixDirectoryAttributes(subDir);
            }

            foreach(var file in directory.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
                file.IsReadOnly = false;
            }
        }
    }
}
