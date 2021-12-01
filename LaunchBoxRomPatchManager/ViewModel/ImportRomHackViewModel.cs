using LaunchBoxRomPatchManager.DataProvider;
using LaunchBoxRomPatchManager.Event;
using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.Model;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace LaunchBoxRomPatchManager.ViewModel
{
    public class ImportRomHackViewModel : ViewModelBase.ViewModelBase
    {
        private EventAggregator eventAggregator;
        private IGame selectedGame;
        private Patcher selectedPatcher;
        private string selectedPatchFilePath;
        private string romHackTitle;
        private PatcherPlatform romHackPlatform;

        public ObservableCollection<ImageToCopy> ImagesToCopy { get; }
        public ObservableCollection<VideoToCopy> VideosToCopy { get; }
        public ObservableCollection<AdditionalAppToCopy> AdditionalAppsToCopy { get; }

        public ObservableCollection<PatcherPlatform> PlatformLookup { get; }

        private ImageToCopy selectedImageToCopy;
        private VideoToCopy selectedVideoToCopy;
        private AdditionalAppToCopy selectedAdditionalAppToCopy;

        private Visibility imagesToCopyVisibility;
        private Visibility videosToCopyVisibility;
        private Visibility additionalAppsToCopyVisibility;
        private Visibility createRomHackLogVisibility;

        private string errorMessage;

        private string createRomHackLog;






        public ICommand SelectPatchFileCommand { get; }
        public ICommand SelectAllImagesCommand { get; }
        public ICommand SelectNoImagesCommand { get; }
        public ICommand SelectAllVideosCommand { get; }
        public ICommand SelectNoVideosCommand { get; }
        public ICommand SelectAllAdditionalAppsCommand { get; }
        public ICommand SelectNoAdditionalAppsCommand { get; }
        public ICommand CreateRomHackCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CloseRomHackingLogCommand { get; }

        public ImportRomHackViewModel(IGame _selectedGame)
        {
            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;

            SelectedGame = _selectedGame;

            ImagesToCopy = new ObservableCollection<ImageToCopy>();
            VideosToCopy = new ObservableCollection<VideoToCopy>();
            AdditionalAppsToCopy = new ObservableCollection<AdditionalAppToCopy>();
            PlatformLookup = new ObservableCollection<PatcherPlatform>();

            CreateRomHackLogVisibility = Visibility.Collapsed;

            SelectPatchFileCommand = new DelegateCommand(OnSelectPatchFileExecute);
            SelectAllImagesCommand = new DelegateCommand(OnSelectAllImagesExecute);
            SelectNoImagesCommand = new DelegateCommand(OnSelectNoImagesExecute);
            SelectAllVideosCommand = new DelegateCommand(OnSelectAllVideosExecute);
            SelectNoVideosCommand = new DelegateCommand(OnSelectNoVideosExecute);
            SelectAllAdditionalAppsCommand = new DelegateCommand(OnSelectAllAdditionalAppsExecute);
            SelectNoAdditionalAppsCommand = new DelegateCommand(OnSelectNoAdditionalAppsExecute);
            CreateRomHackCommand = new DelegateCommand(OnCreateRomHackExecuteAsync, CanCreateRomHackExecute);
            CancelCommand = new DelegateCommand(OnCancelExecute);
            CloseRomHackingLogCommand = new DelegateCommand(OnCloseRomHackingLogExecute);
        }

        private void OnCloseRomHackingLogExecute()
        {
            CreateRomHackLogVisibility = Visibility.Collapsed;
        }

        public async void LoadAsync()
        {
            await InitializePatcherAsync();

            // default RomHackPlatform from SelectedGame
            RomHackPlatform = new PatcherPlatform() { PlatformName = SelectedGame.Platform };

            // initialize ImagesToCopy from SelectedGame all images 
            ImageDetails[] imageDetails = SelectedGame.GetAllImagesWithDetails();
            IOrderedEnumerable<ImageDetails> imageDetailsQuery = imageDetails.OrderBy(id => id.ImageType).ThenBy(id => id.Region);
            foreach (ImageDetails imageDetail in imageDetailsQuery)
            {
                ImagesToCopy.Add(new ImageToCopy() { ImageDetails = imageDetail });
            }

            // initialize VideosToCopy from SelectedGame all videos             
            // Recording, Theme Video, Trailer, Video Snap, Marquee
            string videoPath = SelectedGame.GetVideoPath();
            if (!string.IsNullOrWhiteSpace(videoPath))
            {
                VideosToCopy.Add(new VideoToCopy() { VideoPath = videoPath, VideoType = "" });
            }

            videoPath = SelectedGame.GetThemeVideoPath();
            if (!string.IsNullOrWhiteSpace(videoPath))
            {
                VideosToCopy.Add(new VideoToCopy() { VideoPath = videoPath, VideoType = "Theme" });
            }

            // Initialize AdditionalAppsToCopy from SelectedGame all additional apps 
            IAdditionalApplication[] additionalApplications = SelectedGame.GetAllAdditionalApplications();
            foreach (IAdditionalApplication additionalApplication in additionalApplications)
            {
                AdditionalAppsToCopy.Add(new AdditionalAppToCopy() { AdditionalApplication = additionalApplication });
            }

            ImagesToCopyVisibility = ImagesToCopy.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            VideosToCopyVisibility = VideosToCopy.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            AdditionalAppsToCopyVisibility = AdditionalAppsToCopy.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            // populate the platform lookup
            IPlatform[] platforms = PluginHelper.DataManager.GetAllPlatforms().OrderBy(p => p.Name).ToArray();
            foreach(IPlatform platform in platforms)
            {
                PlatformLookup.Add(new PatcherPlatform() { PlatformName = platform.Name });
            }
        }

        private async Task InitializePatcherAsync()
        {
            // derive SelectedPatcher from SelectedGame's platform 
            PatcherDataProvider patcherDataProvider = new PatcherDataProvider();
            IEnumerable<Patcher> patchers = await patcherDataProvider.GetAllPatchersAsync();

            PatcherPlatform patcherPlatform = new PatcherPlatform() { PlatformName = SelectedGame.Platform };
            PatcherPlatformComparer patcherPlatformComparer = new PatcherPlatformComparer();

            Patcher patcher = patchers.SingleOrDefault(p => p.Platforms.Contains(patcherPlatform, patcherPlatformComparer));

            SelectedPatcher = patcher;
        }

        private void OnCancelExecute()
        {
            eventAggregator.GetEvent<ImportRomHackCancel>().Publish();
        }

        private bool CanCreateRomHackExecute()
        {
            // todo: check if it's safe to create the rom hack
            return true;
        }

        private async void OnCreateRomHackExecuteAsync()
        {
            bool success = false;

            // clear the log and make it visible
            CreateRomHackLog = string.Empty;
            CreateRomHackLogVisibility = Visibility.Visible;

            await Task.Run(() =>
            {
                string patchedRomFilePath;
                try
                {
                    CreateRomHackLog = string.Format($"Creating rom hack for {RomHackTitle}\n{CreateRomHackLog}");
                    patchedRomFilePath = CreateRomHack();

                    if (string.IsNullOrWhiteSpace(patchedRomFilePath))
                    {
                        throw new Exception("Patching process aborted because patched rom file path is empty");
                    }
                }
                catch (Exception ex)
                {
                    CreateRomHackLog = string.Format($"An unexpected error occurred while creating the rom hack: {ex.Message}\n{CreateRomHackLog}");
                    LogHelper.LogException(ex, $"Creating rom hack for {SelectedPatchFilePath}");
                    return;
                }

                IGame newGame = Unbroken.LaunchBox.Plugins.PluginHelper.DataManager.AddNewGame(RomHackTitle);
                try
                {
                    CreateRomHackLog = string.Format($"Creating {RomHackTitle}\n{CreateRomHackLog}");
                    
                    newGame.AggressiveWindowHiding = SelectedGame.AggressiveWindowHiding;
                    newGame.ApplicationPath = patchedRomFilePath;
                    newGame.CommandLine = SelectedGame.CommandLine;
                    newGame.Developer = SelectedGame.Developer;
                    newGame.DisableShutdownScreen = SelectedGame.DisableShutdownScreen;
                    newGame.EmulatorId = SelectedGame.EmulatorId;
                    newGame.GenresString = SelectedGame.GenresString;
                    newGame.HideAllNonExclusiveFullscreenWindows = SelectedGame.HideAllNonExclusiveFullscreenWindows;
                    newGame.HideMouseCursorInGame = SelectedGame.HideMouseCursorInGame;

                    // todo: look into how we can set the dbid
                    newGame.LaunchBoxDbId = null;

                    newGame.MaxPlayers = SelectedGame.MaxPlayers;
                    newGame.Notes = SelectedGame.Notes;
                    newGame.OverrideDefaultStartupScreenSettings = SelectedGame.OverrideDefaultStartupScreenSettings;
                    newGame.Platform = RomHackPlatform.PlatformName;
                    newGame.PlayMode = SelectedGame.PlayMode;
                    newGame.Portable = SelectedGame.Portable;
                    newGame.Publisher = SelectedGame.Publisher;
                    newGame.Region = SelectedGame.Region;
                    newGame.ReleaseDate = selectedGame.ReleaseDate;
                    newGame.ReleaseType = selectedGame.ReleaseType;
                    newGame.ReleaseYear = selectedGame.ReleaseYear;
                    newGame.Series = selectedGame.Series;
                    newGame.ShowBack = selectedGame.ShowBack;
                    newGame.StartupLoadDelay = selectedGame.StartupLoadDelay;
                    newGame.UseStartupScreen = selectedGame.UseStartupScreen;
                }
                catch (Exception ex)
                {
                    CreateRomHackLog = string.Format($"An unexpected error occurred while creating the rom hack in the local LaunchBox database: {ex.Message}\n{CreateRomHackLog}");
                    LogHelper.LogException(ex, $"Creating launchbox game for {SelectedPatchFilePath}");
                    return;
                }

                // add images
                IEnumerable<ImageToCopy> imagesToCopyQuery = ImagesToCopy.Where(itc => itc.Copy == true).OrderBy(itc => itc.ImageDetails.ImageType);

                string cleanTitle = DirectoryInfoHelper.GetCleanFileName(RomHackTitle);
                string fmt = "00";

                try
                {                                        
                    int imageTypeCount = 0;
                    string imageType = string.Empty;
                    foreach (ImageToCopy imageToCopy in imagesToCopyQuery)
                    {
                        if(imageType != imageToCopy.ImageDetails.ImageType)
                        {
                            imageTypeCount = 1;
                        }
                        else
                        {
                            imageTypeCount++;
                        }
                        imageType = imageToCopy.ImageDetails.ImageType;

                        CreateRomHackLog = string.Format($"Image to copy: {imageToCopy.ImageDetails.ImageType}, {imageToCopy.ImageDetails.Region}, {imageToCopy.ImageDetails.FilePath}\n{CreateRomHackLog}");

                        string imagePath = Path.Combine(DirectoryInfoHelper.Instance.LaunchboxImagesPath, RomHackPlatform.PlatformName, imageToCopy.ImageDetails.ImageType);
                        DirectoryInfoHelper.CreateDirectoryIfNotExists(imagePath);
                        
                        if(!string.IsNullOrWhiteSpace(imageToCopy.ImageDetails.Region))
                        {
                            imagePath = Path.Combine(imagePath, imageToCopy.ImageDetails.Region);
                            DirectoryInfoHelper.CreateDirectoryIfNotExists(imagePath);
                        }

                        imagePath = Path.Combine(imagePath, cleanTitle + "-" + imageTypeCount.ToString(fmt) + Path.GetExtension(imageToCopy.ImageDetails.FilePath));
                        DirectoryInfoHelper.CreateDirectoryIfNotExists(imagePath);

                        CreateRomHackLog = string.Format($"Destination path: {imagePath}\n{CreateRomHackLog}");

                        if (!File.Exists(imagePath))
                        {
                            File.Copy(imageToCopy.ImageDetails.FilePath, imagePath);
                        }
                    }
                }
                catch(Exception ex)
                {
                    CreateRomHackLog = string.Format($"An unexpected error occurred while copying images for {RomHackTitle}: {ex.Message}\n{CreateRomHackLog}");
                    LogHelper.LogException(ex, $"Copying images for {RomHackTitle}");
                    return;
                }

                // add selected videos
                IEnumerable<VideoToCopy> videosToCopyQuery = VideosToCopy.Where(v => v.Copy == true).OrderBy(v => v.VideoType);

                try
                {
                    string videoType = string.Empty;
                    int videoTypeCount = 0;
                    foreach (VideoToCopy videoToCopy in videosToCopyQuery)
                    {
                        if (videoType != videoToCopy.VideoType)
                        {
                            videoTypeCount = 1;
                        }
                        else
                        {
                            videoTypeCount++;
                        }
                        videoType = videoToCopy.VideoType;

                        CreateRomHackLog = string.Format($"Video to copy: {videoToCopy.VideoType}, {videoToCopy.VideoPath}\n{CreateRomHackLog}");
                        string videoPath = Path.Combine(DirectoryInfoHelper.Instance.LaunchboxVideosPath, RomHackPlatform.PlatformName);
                        DirectoryInfoHelper.CreateDirectoryIfNotExists(videoPath);

                        if (!string.IsNullOrWhiteSpace(videoToCopy.VideoType))
                        {
                            videoPath = Path.Combine(videoPath, videoToCopy.VideoType);
                            DirectoryInfoHelper.CreateDirectoryIfNotExists(videoPath);
                        }

                        videoPath = Path.Combine(videoPath, cleanTitle + "-" + videoTypeCount.ToString(fmt) + Path.GetExtension(videoToCopy.VideoPath));
                        DirectoryInfoHelper.CreateDirectoryIfNotExists(videoPath);
                        CreateRomHackLog = string.Format($"Destination video path: {videoPath}\n{CreateRomHackLog}");

                        if (!File.Exists(videoPath))
                        {
                            File.Copy(videoToCopy.VideoPath, videoPath);
                        }
                    }
                }
                catch(Exception ex)
                {
                    CreateRomHackLog = string.Format($"An unexpected error occurred while copying videos for {RomHackTitle}: {ex.Message}\n{CreateRomHackLog}");
                    LogHelper.LogException(ex, $"Copying videos for {RomHackTitle}");
                    return;
                }

                // add additional apps - similar to above for additional apps 
                IEnumerable<AdditionalAppToCopy> additionalAppsToCopyQuery = AdditionalAppsToCopy.Where(a => a.Copy == true);
                try
                {
                    foreach(AdditionalAppToCopy additionalAppToCopy in additionalAppsToCopyQuery)
                    {
                        CreateRomHackLog = string.Format($"Copying additional app: {additionalAppToCopy.AdditionalApplication.Name}\n{CreateRomHackLog}");

                        IAdditionalApplication newAdditionalApp = newGame.AddNewAdditionalApplication();
                        newAdditionalApp.ApplicationPath = additionalAppToCopy.AdditionalApplication.ApplicationPath;
                        newAdditionalApp.AutoRunAfter = additionalAppToCopy.AdditionalApplication.AutoRunAfter;
                        newAdditionalApp.AutoRunBefore = additionalAppToCopy.AdditionalApplication.AutoRunBefore;
                        newAdditionalApp.CommandLine = additionalAppToCopy.AdditionalApplication.CommandLine;
                        newAdditionalApp.Developer = additionalAppToCopy.AdditionalApplication.Developer;
                        newAdditionalApp.Disc = additionalAppToCopy.AdditionalApplication.Disc;
                        newAdditionalApp.EmulatorId = additionalAppToCopy.AdditionalApplication.EmulatorId;                        
                        newAdditionalApp.Installed = additionalAppToCopy.AdditionalApplication.Installed;
                        newAdditionalApp.LastPlayed = additionalAppToCopy.AdditionalApplication.LastPlayed;
                        newAdditionalApp.Name = additionalAppToCopy.AdditionalApplication.Name;
                        newAdditionalApp.PlayCount = additionalAppToCopy.AdditionalApplication.PlayCount;
                        newAdditionalApp.Priority = additionalAppToCopy.AdditionalApplication.Priority;
                        newAdditionalApp.Publisher = additionalAppToCopy.AdditionalApplication.Publisher;
                        newAdditionalApp.Region = additionalAppToCopy.AdditionalApplication.Region;
                        newAdditionalApp.ReleaseDate = additionalAppToCopy.AdditionalApplication.ReleaseDate;
                        newAdditionalApp.SideA = additionalAppToCopy.AdditionalApplication.SideA;
                        newAdditionalApp.SideB = additionalAppToCopy.AdditionalApplication.SideB;
                        newAdditionalApp.Status = additionalAppToCopy.AdditionalApplication.Status;
                        newAdditionalApp.UseDosBox = additionalAppToCopy.AdditionalApplication.UseDosBox;
                        newAdditionalApp.UseEmulator = additionalAppToCopy.AdditionalApplication.UseEmulator;
                        newAdditionalApp.Version = additionalAppToCopy.AdditionalApplication.Version;
                        newAdditionalApp.WaitForExit = additionalAppToCopy.AdditionalApplication.WaitForExit;                        
                    }
                }
                catch(Exception ex)
                {
                    CreateRomHackLog = string.Format($"An unexpected error occurred while copying additional applicatoins for {RomHackTitle}: {ex.Message}\n{CreateRomHackLog}");
                    LogHelper.LogException(ex, $"Copying additional apps for {RomHackTitle}");
                    return;
                }

                // save data and refresh
                try
                {
                    CreateRomHackLog = string.Format($"Saving {RomHackTitle}\n{CreateRomHackLog}");
                    PluginHelper.DataManager.Save(true);
                }
                catch (Exception ex)
                {
                    CreateRomHackLog = string.Format($"An unexpected error occurred while saving {RomHackTitle} to local LaunchBox database: {ex.Message}\n{CreateRomHackLog}");
                    LogHelper.LogException(ex, $"Saving {RomHackTitle} to local LaunchBox database");
                    return;
                }

                try
                {
                    CreateRomHackLog = string.Format($"Refreshing LaunchBox\n{CreateRomHackLog}");
                    PluginHelper.DataManager.ForceReload();
                    PluginHelper.LaunchBoxMainViewModel.RefreshData();
                }
                catch(Exception ex)
                {
                    CreateRomHackLog = string.Format($"An unexpected error occurred while refreshing LaunchBox: {ex.Message}\n{CreateRomHackLog}");
                    LogHelper.LogException(ex, $"Refreshing launchbox");
                    return;
                }

                success = true;
            });

            // close 
            if (success)
            {
                CreateRomHackLogVisibility = Visibility.Collapsed;
                eventAggregator.GetEvent<ImportRomHackCancel>().Publish();
            }
        }

        private string CopyOriginalRomToTemp(string tempDirectory)
        {
            // make a copy of the source rom in a temp directory 
            string originalRomFileName = Path.GetFileName(SelectedGame.ApplicationPath);
            string romFullPathInTempDirectory = Path.Combine(tempDirectory, originalRomFileName);
            File.Copy(SelectedGame.ApplicationPath, romFullPathInTempDirectory, true);

            return romFullPathInTempDirectory;
        }

        private string CreateRomHack()
        {
            // since rom hack title is used in folder and file names, get a clean version without illegal characters
            string cleanRomHackTitle;
            try
            {
                CreateRomHackLog = string.Format($"Cleaning rom hack title: {RomHackTitle}\n{CreateRomHackLog}");

                cleanRomHackTitle = DirectoryInfoHelper.GetCleanFileName(RomHackTitle);
                
                CreateRomHackLog = string.Format($"Cleaned rom hack title: {cleanRomHackTitle}\n{CreateRomHackLog}");
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while cleaning the rom hack title: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Cleaning rom hack title for {RomHackTitle}");
                return string.Empty;
            }

            // create temp dir in ..\plugins\this plugin\temp\guid
            string tempDirectory;
            try
            {
                CreateRomHackLog = string.Format($"Creating temporary directory\n{CreateRomHackLog}");
                tempDirectory = CreateTempDirectory();
                CreateRomHackLog = string.Format($"Temporary directory created: {tempDirectory}\n{CreateRomHackLog}");
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while creating the temporary directory: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Creating temporary directory for {RomHackTitle}");
                return string.Empty;
            }

            // make a copy of the source rom in a temp directory 
            string romFullPathInTempDirectory;
            try
            {
                CreateRomHackLog = string.Format($"Copying source rom file to {tempDirectory}\n{CreateRomHackLog}");
                romFullPathInTempDirectory = CopyOriginalRomToTemp(tempDirectory);
                CreateRomHackLog = string.Format($"Copied source rom to temporary directory: {romFullPathInTempDirectory}\n{CreateRomHackLog}");
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while copying the source rom to the temporary directory: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Copying source rom to temporary directory for {RomHackTitle}");
                return string.Empty;
            }

            // check if the source rom is zipped 
            bool isZipped = false;
            try
            {
                CreateRomHackLog = string.Format($"Checking if source rom is a zip file: {romFullPathInTempDirectory}\n{CreateRomHackLog}");
                if (string.Compare(Path.GetExtension(romFullPathInTempDirectory), ".zip", true) == 0)
                {
                    isZipped = true;
                }
                string tempMessage = isZipped ? "Source rom is a zip file" : "Source rom is not a zip file";
                CreateRomHackLog = string.Format($"{tempMessage}\n{CreateRomHackLog}");
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while checking whether the source rom is a zip file: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Checking for zip extension on {romFullPathInTempDirectory}");
                return string.Empty;
            }

            // create a temporary directory  for zip file if necessary 
            string tempZipDirectory = string.Empty;
            if (isZipped)
            {
                try
                {
                    tempZipDirectory = Path.Combine(tempDirectory, cleanRomHackTitle);

                    CreateRomHackLog = string.Format($"Creating directory to unzip source rom: {tempZipDirectory}\n{CreateRomHackLog}");

                    if (!Directory.Exists(tempZipDirectory))
                    {
                        Directory.CreateDirectory(tempZipDirectory);
                    }
                }
                catch (Exception ex)
                {
                    CreateRomHackLog = string.Format($"An unexpected error occurred while creating a directory to unzip the source rom file: {ex.Message}\n{CreateRomHackLog}");
                    LogHelper.LogException(ex, $"Attempting to create directory to unzip source rom: {tempZipDirectory}");
                    return string.Empty;
                }
            }

            // unzip source rom if it's a zip
            if (isZipped)
            {
                try
                {
                    CreateRomHackLog = string.Format($"Unzipping {romFullPathInTempDirectory} to {tempZipDirectory}\n{CreateRomHackLog}");
                    ZipFile.ExtractToDirectory(romFullPathInTempDirectory, tempZipDirectory);
                }
                catch (Exception ex)
                {
                    CreateRomHackLog = string.Format($"An unexpected error occurred while trying to unzip {romFullPathInTempDirectory} to {tempZipDirectory}: {ex.Message}\n{CreateRomHackLog}");
                    LogHelper.LogException(ex, $"Attempting to extract {romFullPathInTempDirectory} to {tempZipDirectory}");
                    return string.Empty;
                }
            }

            // get a handle on the extracted ROM file 
            if (isZipped)
            {
                CreateRomHackLog = string.Format($"Getting a handle on the extracted ROM file in {tempZipDirectory}\n{CreateRomHackLog}");
                try
                {
                    bool unzippedRomFileFound = false;

                    IEnumerable<string> filesInDirectory = Directory.EnumerateFiles(tempZipDirectory);
                    foreach (string file in filesInDirectory)
                    {
                        // take the first file that is not a zip
                        // todo: will run into trouble here if patching a game zip that has multiple files
                        if (string.Compare(Path.GetExtension(file), ".zip", true) != 0)
                        {
                            romFullPathInTempDirectory = file;
                            unzippedRomFileFound = true;
                            break;
                        }
                    }

                    if (!unzippedRomFileFound)
                    {
                        throw new Exception($"No file was found in {tempZipDirectory}");
                    }
                }
                catch (Exception ex)
                {
                    CreateRomHackLog = string.Format($"An unexpected error occurred while trying to get a handle on the extracted rom file: {ex.Message}\n{CreateRomHackLog}");
                    LogHelper.LogException(ex, $"Attempting to get a handle on the extracted rom file in {tempZipDirectory}");
                    return string.Empty;
                }
            }

            // rename source rom to the patch name 
            string romCopyExtension = Path.GetExtension(romFullPathInTempDirectory);
            string romCopyPath = Path.GetDirectoryName(romFullPathInTempDirectory);
            string romCopyNewName = Path.Combine(romCopyPath, $"{cleanRomHackTitle}{romCopyExtension}");
            try
            {
                CreateRomHackLog = string.Format($"Renaming {romFullPathInTempDirectory} to {romCopyNewName}\n{CreateRomHackLog}");

                // rename the rom to the patch name
                File.Move(romFullPathInTempDirectory, romCopyNewName);
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while attempting to rename {romFullPathInTempDirectory} to {romCopyNewName}: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Attempting to rename the source rom from {romFullPathInTempDirectory} to {romCopyNewName}");
                return string.Empty;
            }

            // copy the Rom Patch into the temp directory
            string romPatchCopyFileName = Path.GetFileName(SelectedPatchFilePath);
            string romPatchCopyDestinationFullPath = Path.Combine(tempDirectory, romPatchCopyFileName);
            try
            {
                CreateRomHackLog = string.Format($"Copying rom patch from {SelectedPatchFilePath} to {romPatchCopyDestinationFullPath}\n{CreateRomHackLog}");
                File.Copy(SelectedPatchFilePath, romPatchCopyDestinationFullPath, true);
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while attempting to copy the rom patch from {SelectedPatchFilePath} to {romPatchCopyDestinationFullPath}: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Attempting to copy {SelectedPatchFilePath} to {romPatchCopyDestinationFullPath}");
                return string.Empty;
            }

            // get command line arguments and replace the actual rom and patch paths 
            string commandLineArgs = SelectedPatcher.CommandLine;
            try
            {
                commandLineArgs = commandLineArgs.Replace("{patch}", $"\"{romPatchCopyDestinationFullPath}\"");
                commandLineArgs = commandLineArgs.Replace("{rom}", $"\"{romCopyNewName}\"");

                CreateRomHackLog = string.Format($"Command line argument: {commandLineArgs}\n{CreateRomHackLog}");
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while attempting create command line arguments for {commandLineArgs}: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Attempting to create command line arguments");
                return string.Empty;
            }

            // apply patch 
            try
            {
                CreateRomHackLog = string.Format($"Creating rom patch using {SelectedPatcher.Path} and {commandLineArgs}\n{ CreateRomHackLog}");

                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = SelectedPatcher.Path,
                        Arguments = commandLineArgs,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                proc.Start();

                string processOutput = proc.StandardOutput.ReadToEnd();
                string processError = proc.StandardError.ReadToEnd();

                proc.WaitForExit();

                CreateRomHackLog = string.Format($"Rom patcher output: {processOutput}\n{CreateRomHackLog}");

                if (!string.IsNullOrWhiteSpace(processError))
                {
                    throw new Exception($"Patch process returned the following error: {processError}");
                }
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while attempting to create the rom patch using {SelectedPatcher.Path} and arguments {commandLineArgs}: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Attempting to patch rom using {SelectedPatcher.Path} with arguments {commandLineArgs}");
                return string.Empty;
            }

            string patchedRomFilePath = romCopyNewName;

            // zip patched application if original was zipped
            if (isZipped)
            {
                try
                {
                    patchedRomFilePath = Path.Combine(tempDirectory, $"{cleanRomHackTitle}.zip");

                    CreateRomHackLog = string.Format($"Zipping {tempZipDirectory} to {patchedRomFilePath}\n{CreateRomHackLog}");

                    ZipFile.CreateFromDirectory(tempZipDirectory, patchedRomFilePath);
                }
                catch (Exception ex)
                {
                    CreateRomHackLog = string.Format($"An unexpected error occurred while attempting to create zip file from {tempZipDirectory} to {patchedRomFilePath}: {ex.Message}\n{CreateRomHackLog}");
                    LogHelper.LogException(ex, $"Attempting to zip {tempZipDirectory} to {patchedRomFilePath}");
                    return string.Empty;
                }
            }

            CreateRomHackLog = string.Format($"Patched rom file: {patchedRomFilePath}\n{CreateRomHackLog}");

            // create plug-in folder if it doesn't already exist 
            try
            {
                if (!Directory.Exists(DirectoryInfoHelper.Instance.PluginFolder))
                {
                    CreateRomHackLog = string.Format($"Creating plugin folder: {DirectoryInfoHelper.Instance.PluginFolder}\n{CreateRomHackLog}");
                    Directory.CreateDirectory(DirectoryInfoHelper.Instance.PluginFolder);
                }
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while attempting to check/create folder {DirectoryInfoHelper.Instance.PluginFolder}: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Attempting to check/create folder {DirectoryInfoHelper.Instance.PluginFolder}");
                return string.Empty;
            }

            // create ../plugin/rom hacks if it doesn't exist
            string pluginRomHacksDirectory = Path.Combine(DirectoryInfoHelper.Instance.PluginFolder, "Rom Hacks");
            try
            {
                if (!Directory.Exists(pluginRomHacksDirectory))
                {
                    CreateRomHackLog = string.Format($"Creating plugin rom hacks folder: {pluginRomHacksDirectory}\n{CreateRomHackLog}");
                    Directory.CreateDirectory(pluginRomHacksDirectory);
                }
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while attempting to check/create folder {pluginRomHacksDirectory}: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Attempting to check/create folder {pluginRomHacksDirectory}");
                return string.Empty;
            }

            // create ../plugin/rom hacks/{rom hack platform} if it doesn't exist
            string pluginRomHacksPlatformDirectory = Path.Combine(pluginRomHacksDirectory, RomHackPlatform.PlatformName);
            try
            {
                if (!Directory.Exists(pluginRomHacksPlatformDirectory))
                {
                    CreateRomHackLog = string.Format($"Creating plugin rom hacks platform folder: {pluginRomHacksPlatformDirectory}\n{CreateRomHackLog}");
                    Directory.CreateDirectory(pluginRomHacksPlatformDirectory);
                }
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while attempting to check/create folder {pluginRomHacksPlatformDirectory}: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Attempting to check/create folder {pluginRomHacksPlatformDirectory}");
                return string.Empty;
            }

            // create ../plugin/rom hacks/{rom hack platform}/{rom hack title} if it doesn't exist 
            string pluginRomHacksPlatformGameDirectory = Path.Combine(pluginRomHacksPlatformDirectory, cleanRomHackTitle);
            try
            {
                if (!Directory.Exists(pluginRomHacksPlatformGameDirectory))
                {
                    CreateRomHackLog = string.Format($"Creating plugin rom hacks game folder {pluginRomHacksPlatformGameDirectory}\n{CreateRomHackLog}");
                    Directory.CreateDirectory(pluginRomHacksPlatformGameDirectory);
                }
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while attempting to check/create folder {pluginRomHacksPlatformGameDirectory}: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Attempting to check/create folder {pluginRomHacksPlatformGameDirectory}");
                return string.Empty;
            }

            // fix file and folder attributes 
            try
            {
                CreateRomHackLog = string.Format($"Fixing file attributes on {tempDirectory}\n{CreateRomHackLog}");
                DirectoryInfoHelper.FixDirectoryAttributes(new DirectoryInfo(tempDirectory));
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while attempting to fix directory attributes on {tempDirectory}: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Attempting to fix attributes on folder {tempDirectory}");
                return string.Empty;
            }

            // move patch files to plugin/patches/{platform}/{rom hack title} ? 

            // copy the patch to the game directory
            string romPatchFinalDestinationFileName = Path.Combine(pluginRomHacksPlatformGameDirectory, romPatchCopyFileName);
            try
            {
                CreateRomHackLog = string.Format($"Copying patch to plugin rom hacks game folder {romPatchCopyDestinationFullPath} to {romPatchFinalDestinationFileName}\n{CreateRomHackLog}");

                File.Copy(romPatchCopyDestinationFullPath, romPatchFinalDestinationFileName, true);
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while attempting to copy rom patch to game folder from {romPatchCopyDestinationFullPath} to {romPatchFinalDestinationFileName}: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Attempting to copy rom patch to game folder from {romPatchCopyDestinationFullPath} to {romPatchFinalDestinationFileName}");
                return string.Empty;
            }

            // copy the patched game to the game directory 
            // ..\LaunchBox\Games\{Platform}\
            string finalRomHackFileName = Path.GetFileName(patchedRomFilePath);
            string patchedGameFinalDestinationFileName = Path.Combine(DirectoryInfoHelper.Instance.LaunchboxGamesPath, RomHackPlatform.PlatformName, finalRomHackFileName);
            DirectoryInfoHelper.CreateDirectoryIfNotExists(patchedGameFinalDestinationFileName);
            try
            {
                File.Copy(patchedRomFilePath, patchedGameFinalDestinationFileName, true);
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while attempting to copy patched rom to game folder from {patchedRomFilePath} to {patchedGameFinalDestinationFileName}: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, $"Attempting to copy patched rom to game folder from {patchedRomFilePath} to {patchedGameFinalDestinationFileName}");
                return string.Empty;
            }

            // delete temp dir
            try
            {
                Directory.Delete(DirectoryInfoHelper.Instance.PluginTempFolder, true);
            }
            catch (Exception ex)
            {
                CreateRomHackLog = string.Format($"An unexpected error occurred while attempting to delete the temp directory {DirectoryInfoHelper.Instance.PluginTempFolder}: {ex.Message}\n{CreateRomHackLog}");
                LogHelper.LogException(ex, "Deleting temp directory");
            }

            return patchedGameFinalDestinationFileName;
        }

        private static string CreateTempDirectory()
        {
            // make sure plugin folder exists 
            string pluginFolder = DirectoryInfoHelper.Instance.PluginFolder;
            DirectoryInfoHelper.CreateDirectoryIfNotExists(pluginFolder);

            // make sure temp folder exists 
            string pluginTempFolder = DirectoryInfoHelper.Instance.PluginTempFolder;
            DirectoryInfoHelper.CreateDirectoryIfNotExists(pluginTempFolder);

            // create a new unique folder to work out of in the temp directory
            string tempDirGuid = Guid.NewGuid().ToString();
            string tempDir = Path.Combine(pluginTempFolder, tempDirGuid);
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            return tempDir;                       
        }

        private void OnSelectNoAdditionalAppsExecute()
        {
            AdditionalAppToCopy[] tempAdditionalAppTopCopy = AdditionalAppsToCopy.ToArray();

            AdditionalAppsToCopy.Clear();
            foreach (AdditionalAppToCopy additionalAppToCopy in tempAdditionalAppTopCopy)
            {
                AdditionalAppsToCopy.Add(new AdditionalAppToCopy()
                {
                    AdditionalApplication = additionalAppToCopy.AdditionalApplication,
                    Copy = false
                });
            }
        }

        private void OnSelectAllAdditionalAppsExecute()
        {
            AdditionalAppToCopy[] tempAdditionalAppTopCopy = AdditionalAppsToCopy.ToArray();

            AdditionalAppsToCopy.Clear();
            foreach (AdditionalAppToCopy additionalAppToCopy in tempAdditionalAppTopCopy)
            {
                AdditionalAppsToCopy.Add(new AdditionalAppToCopy()
                {
                    AdditionalApplication = additionalAppToCopy.AdditionalApplication,
                    Copy = true
                });
            }
        }

        private void OnSelectNoVideosExecute()
        {
            VideoToCopy[] tempVideosToCopy = VideosToCopy.ToArray();

            VideosToCopy.Clear();
            foreach (VideoToCopy videoToCopy in tempVideosToCopy)
            {
                VideosToCopy.Add(new VideoToCopy()
                {
                    VideoType = videoToCopy.VideoType,
                    VideoPath = videoToCopy.VideoPath,
                    Copy = false
                });
            }
        }

        private void OnSelectAllVideosExecute()
        {
            VideoToCopy[] tempVideosToCopy = VideosToCopy.ToArray();

            VideosToCopy.Clear();
            foreach (VideoToCopy videoToCopy in tempVideosToCopy)
            {
                VideosToCopy.Add(new VideoToCopy()
                {
                    VideoType = videoToCopy.VideoType,
                    VideoPath = videoToCopy.VideoPath,
                    Copy = true
                });
            }
        }

        private void OnSelectNoImagesExecute()
        {
            ImageToCopy[] tempImageToCopy = ImagesToCopy.ToArray();

            ImagesToCopy.Clear();
            foreach (ImageToCopy imageToCopy in tempImageToCopy)
            {
                ImagesToCopy.Add(new ImageToCopy() { ImageDetails = imageToCopy.ImageDetails, Copy = false });
            }
        }

        private void OnSelectAllImagesExecute()
        {
            ImageToCopy[] tempImageToCopy = ImagesToCopy.ToArray();

            ImagesToCopy.Clear();
            foreach (ImageToCopy imageToCopy in tempImageToCopy)
            {
                ImagesToCopy.Add(new ImageToCopy() { ImageDetails = imageToCopy.ImageDetails, Copy = true });
            }
        }

        private void OnSelectPatchFileExecute()
        {
            OpenFileDialog openFileDialog;
            openFileDialog = new OpenFileDialog();

            string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            if (Directory.Exists(downloadsFolder))
            {
                openFileDialog.InitialDirectory = downloadsFolder;
            }
            else
            {
                openFileDialog.InitialDirectory = DirectoryInfoHelper.Instance.ApplicationPath;
            }

            openFileDialog.Title = "Select patch file";
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedPatchFilePath = openFileDialog.FileName;

                // default RomHackTitle from selected patch file 
                RomHackTitle = Path.GetFileNameWithoutExtension(SelectedPatchFilePath);

                // re-prompt if it's a zip file to select the patch from inside the zip
                if (openFileDialog.FileName.EndsWith(".zip"))
                {
                    openFileDialog.Title = "Select patch file inside the zip";
                    openFileDialog.InitialDirectory = openFileDialog.FileName;
                    if (openFileDialog.ShowDialog() == true)
                    {
                        SelectedPatchFilePath = openFileDialog.FileName;
                    }
                }
            }
        }

        public IGame SelectedGame
        {
            get { return selectedGame; }
            set
            {
                selectedGame = value;
                OnPropertyChanged("SelectedGame");
            }
        }

        public Patcher SelectedPatcher
        {
            get { return selectedPatcher; }
            set
            {
                selectedPatcher = value;
                OnPropertyChanged("SelectedPatcher");
            }
        }

        public string SelectedPatchFilePath
        {
            get { return selectedPatchFilePath; }
            set
            {
                selectedPatchFilePath = value;
                OnPropertyChanged("SelectedPatchFilePath");
            }
        }

        public string RomHackTitle
        {
            get { return romHackTitle; }
            set
            {
                romHackTitle = value;
                OnPropertyChanged("RomHackTitle");
            }
        }

        public PatcherPlatform RomHackPlatform
        {
            get { return romHackPlatform; }
            set
            {
                romHackPlatform = value;
                OnPropertyChanged("RomHackPlatform");
            }
        }

        public ImageToCopy SelectedImageToCopy
        {
            get { return selectedImageToCopy; }
            set
            {
                selectedImageToCopy = value;
                OnPropertyChanged("SelectedImageToCopy");
            }
        }

        public VideoToCopy SelectedVideoToCopy
        {
            get { return selectedVideoToCopy; }
            set
            {
                selectedVideoToCopy = value;
                OnPropertyChanged("SelectedVideoToCopy");
            }
        }

        public AdditionalAppToCopy SelectedAdditionalAppToCopy
        {
            get { return selectedAdditionalAppToCopy; }
            set
            {
                selectedAdditionalAppToCopy = value;
                OnPropertyChanged("SelectedAdditionalAppToCopy");
            }
        }

        public Visibility ImagesToCopyVisibility
        {
            get { return imagesToCopyVisibility; }
            private set
            {
                imagesToCopyVisibility = value;
                OnPropertyChanged("ImagesToCopyVisibility");
            }
        }

        public Visibility VideosToCopyVisibility
        {
            get { return videosToCopyVisibility; }
            private set
            {
                videosToCopyVisibility = value;
                OnPropertyChanged("VideosToCopyVisibility");
            }
        }

        public Visibility AdditionalAppsToCopyVisibility
        {
            get { return additionalAppsToCopyVisibility; }
            private set
            {
                additionalAppsToCopyVisibility = value;
                OnPropertyChanged("AdditionalAppsToCopyVisibility");
            }
        }

        public Visibility CreateRomHackLogVisibility
        {
            get { return createRomHackLogVisibility; }
            private set
            {
                createRomHackLogVisibility = value;
                OnPropertyChanged("CreateRomHackLogVisibility");
            }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        public string CreateRomHackLog
        {
            get { return createRomHackLog; }
            set
            {
                createRomHackLog = value;
                OnPropertyChanged("CreateRomHackLog");
            }
        }
    }
}
