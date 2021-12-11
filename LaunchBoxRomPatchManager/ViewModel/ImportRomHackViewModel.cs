using LaunchBoxRomPatchManager.DataProvider;
using LaunchBoxRomPatchManager.EmbeddedResources;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace LaunchBoxRomPatchManager.ViewModel
{
    public class ImportRomHackViewModel : ViewModelBase.ViewModelBase
    {
        private readonly EventAggregator eventAggregator;

        private Visibility romHackVisibility;
        private Visibility metadataVisibility;
        private Visibility notesVisibility;
        private Visibility additionalAppsVisibility;
        private Visibility mediaVisibility;
        private Visibility imagesVisibility;
        private Visibility videosVisibility;
        private Visibility emulationVisibility;
        private Visibility startupPauseVisibility;
        private Visibility logVisibility;

        public ObservableCollection<ImageToCopy> ImagesToCopy { get; }
        public ObservableCollection<VideoToCopy> VideosToCopy { get; }
        public ObservableCollection<AdditionalAppToCopy> AdditionalAppsToCopy { get; }
        public ObservableCollection<PatcherPlatform> PlatformLookup { get; }
        public ObservableCollection<string> TabPages { get; }

        private IGame selectedGame;
        private ImageToCopy selectedImageToCopy;
        private VideoToCopy selectedVideoToCopy;
        private AdditionalAppToCopy selectedAdditionalAppToCopy;
        private string selectedTabPage;
        private Patcher selectedPatcher;
        private string selectedPatchFilePath;

        private PatcherPlatform romHackPlatform;
        private string romHackTitle;
        private string romHackSortTitle;
        private DateTime? romHackReleaseDate;
        private string romHackRating;
        private string romHackReleaseType;
        private int? romHackMaxPlayers;
        private string romHackGenreString;
        private string romHackDeveloper;
        private string romHackSeries;
        private string romHackRegion;
        private string romHackPlayMode;
        private string romHackVersion;
        private string romHackPublisher;
        private string romHackStatus;
        private string romHackSource;
        private DateTime? romHackDateAdded;
        private string romHackWikipediaUrl;
        private string romHackVideoUrl;
        private float romHackStarRating;
        private int romHackPlayCount;
        private DateTime? romHackLastPlayedDate;
        private bool romHackFavorite;
        private bool romHackPortable;
        private bool romHackCompleted;
        private bool romHackHide;
        private bool romHackBroken;
        private bool? romHackInstalled;
        private string romHackNotes;
        private string romHackLog;

        public ICommand SelectPatchFileCommand { get; }
        public ICommand SelectAllImagesCommand { get; }
        public ICommand SelectNoImagesCommand { get; }
        public ICommand SelectAllVideosCommand { get; }
        public ICommand SelectNoVideosCommand { get; }
        public ICommand SelectAllAdditionalAppsCommand { get; }
        public ICommand SelectNoAdditionalAppsCommand { get; }
        public ICommand CreateRomHackCommand { get; }
        public ICommand CancelCommand { get; }

        public ImportRomHackViewModel(IGame _selectedGame)
        {
            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;

            SelectedGame = _selectedGame;

            TabPages = new ObservableCollection<string>();
            ImagesToCopy = new ObservableCollection<ImageToCopy>();
            VideosToCopy = new ObservableCollection<VideoToCopy>();
            AdditionalAppsToCopy = new ObservableCollection<AdditionalAppToCopy>();
            PlatformLookup = new ObservableCollection<PatcherPlatform>();

            SelectPatchFileCommand = new DelegateCommand(OnSelectPatchFileExecute);
            SelectAllImagesCommand = new DelegateCommand(OnSelectAllImagesExecute);
            SelectNoImagesCommand = new DelegateCommand(OnSelectNoImagesExecute);
            SelectAllVideosCommand = new DelegateCommand(OnSelectAllVideosExecute);
            SelectNoVideosCommand = new DelegateCommand(OnSelectNoVideosExecute);
            SelectAllAdditionalAppsCommand = new DelegateCommand(OnSelectAllAdditionalAppsExecute);
            SelectNoAdditionalAppsCommand = new DelegateCommand(OnSelectNoAdditionalAppsExecute);
            CreateRomHackCommand = new DelegateCommand(OnCreateRomHackExecuteAsync, CanCreateRomHackExecute);
            CancelCommand = new DelegateCommand(OnCancelExecute);
        }

        public async void LoadAsync()
        {
            // create the available tabs
            InitializeTabPages();

            // initialize the patcher to use from the selected game
            await InitializePatcherAsync();

            // populate the platform lookup
            InitializePlatformLookup();

            // initialize rom hack properties from selected game
            InitializeRomHackProperties();
        }


        private void InitializeTabPages()
        {
            SelectedTabPage = "Rom Hack";

            TabPages.Add("Rom Hack");
            TabPages.Add("Metadata");
            TabPages.Add("Notes");
            TabPages.Add("Additional Apps");
            TabPages.Add("Media");
            TabPages.Add("Images");
            TabPages.Add("Videos");
            TabPages.Add("Emulation");
            TabPages.Add("Startup/Pause");
            TabPages.Add("Log");
        }

        // derive the SelectedPatcher from the SelectedGame's platform 
        private async Task InitializePatcherAsync()
        {
            // get all patchers from the data file
            PatcherDataProvider patcherDataProvider = new PatcherDataProvider();
            IEnumerable<Patcher> patchers = await patcherDataProvider.GetAllPatchersAsync();

            // find the patcher that has this platform assigned
            PatcherPlatform patcherPlatform = new PatcherPlatform() { PlatformName = SelectedGame.Platform };
            PatcherPlatformComparer patcherPlatformComparer = new PatcherPlatformComparer();
            Patcher patcher = patchers.SingleOrDefault(p => p.Platforms.Contains(patcherPlatform, patcherPlatformComparer));

            SelectedPatcher = patcher;
        }

        private void InitializePlatformLookup()
        {
            IPlatform[] platforms = PluginHelper.DataManager.GetAllPlatforms().OrderBy(p => p.Name).ToArray();
            foreach (IPlatform platform in platforms)
            {
                PlatformLookup.Add(new PatcherPlatform() { PlatformName = platform.Name });
            }
        }

        private void InitializeRomHackProperties()
        {
            RomHackTitle = SelectedGame.Title;
            RomHackSortTitle = SelectedGame.SortTitle;
            RomHackPlatform = PlatformLookup.FirstOrDefault(p => p.PlatformName == SelectedGame.Platform);
            RomHackReleaseDate = SelectedGame.ReleaseDate;
            RomHackRating = SelectedGame.Rating;
            RomHackReleaseType = SelectedGame.ReleaseType;
            RomHackMaxPlayers = SelectedGame.MaxPlayers;
            RomHackGenreString = SelectedGame.GenresString;
            RomHackDeveloper = SelectedGame.Developer;
            RomHackSeries = SelectedGame.Series;
            RomHackRegion = SelectedGame.Region;
            RomHackPlayMode = SelectedGame.PlayMode;
            RomHackVersion = SelectedGame.Version;
            RomHackPublisher = SelectedGame.Publisher;
            RomHackStatus = SelectedGame.Status;
            RomHackSource = SelectedGame.Source;
            RomHackDateAdded = DateTime.Now;
            RomHackWikipediaUrl = SelectedGame.WikipediaUrl;
            RomHackVideoUrl = SelectedGame.VideoUrl;
            RomHackStarRating = SelectedGame.StarRatingFloat;
            RomHackPlayCount = SelectedGame.PlayCount;
            RomHackLastPlayedDate = SelectedGame.LastPlayedDate;
            RomHackNotes = SelectedGame.Notes;
            RomHackFavorite = SelectedGame.Favorite;
            RomHackPortable = SelectedGame.Portable;
            RomHackCompleted = SelectedGame.Completed;
            RomHackHide = SelectedGame.Hide;
            RomHackBroken = SelectedGame.Broken;
            RomHackInstalled = SelectedGame.Installed;

            // Get the list of images from the selected game
            InitializeImagesToCopy();

            // Get the list of videos from the selected game
            InitializeVideosToCopy();

            // Initialize AdditionalAppsToCopy from SelectedGame all additional apps 
            InitializeAdditionalAppsToCopy();
        }

        private void InitializeImagesToCopy()
        {
            ImageDetails[] imageDetails = SelectedGame.GetAllImagesWithDetails();
            IOrderedEnumerable<ImageDetails> imageDetailsQuery = imageDetails.OrderBy(id => id.ImageType).ThenBy(id => id.Region);
            foreach (ImageDetails imageDetail in imageDetailsQuery)
            {
                ImagesToCopy.Add(new ImageToCopy() { ImageDetails = imageDetail, Copy = true });
            }
        }

        private void InitializeVideosToCopy()
        {
            AddVideosToCopy("Recording");
            AddVideosToCopy("Theme Video");
            AddVideosToCopy("Trailer");
            AddVideosToCopy("Video");
            AddVideosToCopy("Marquee");
        }

        private void AddVideosToCopy(string videoType)
        {
            string videoPath = SelectedGame.GetVideoPath(videoType);

            if (!string.IsNullOrWhiteSpace(videoPath))
            {
                VideosToCopy.Add(new VideoToCopy() { VideoPath = videoPath, VideoType = videoType, Copy = true });
            }
        }

        private void InitializeAdditionalAppsToCopy()
        {
            IAdditionalApplication[] additionalApplications = SelectedGame.GetAllAdditionalApplications();
            foreach (IAdditionalApplication additionalApplication in additionalApplications)
            {
                AdditionalAppsToCopy.Add(new AdditionalAppToCopy() { AdditionalApplication = additionalApplication, Copy = true });
            }
        }

        private void OnCancelExecute()
        {
            // publishes an event that the window subscribes to so it can close the window
            eventAggregator.GetEvent<ImportRomHackCancel>().Publish();
        }

        private bool CanCreateRomHackExecute()
        {
            // check if it's safe to create the rom hack - check required fields 

            // selected patcher cannot be blank
            if (SelectedPatcher == null) return false;

            // selected patcher must have an executable path
            if (string.IsNullOrWhiteSpace(SelectedPatcher?.Path)) return false;

            // selected patch file cannot be blank
            if (string.IsNullOrWhiteSpace(SelectedPatchFilePath)) return false;

            // selected patch file must exist 
            if (!File.Exists(SelectedPatchFilePath)) return false;

            // rom hack title cannot be blank
            if (string.IsNullOrWhiteSpace(RomHackTitle)) return false;
 
            return true;
        }

        private async void OnCreateRomHackExecuteAsync()
        {
            bool success = false;

            // clear the log and make it visible
            RomHackLog = string.Empty;

            SelectedTabPage = "Log";

            await Task.Run(() =>
            {
                string patchedRomFilePath;
                try
                {
                    AddRomHackLogMessage($"Creating rom hack for {RomHackTitle}");

                    patchedRomFilePath = CreateRomHack();

                    if (string.IsNullOrWhiteSpace(patchedRomFilePath))
                    {
                        throw new Exception("Patching process aborted because patched rom file path is empty");
                    }
                }
                catch (Exception ex)
                {
                    AddRomHackLogMessage($"An unexpected error occurred while creating the rom hack: {ex.Message}");
                    LogHelper.LogException(ex, $"Creating rom hack for {SelectedPatchFilePath}");
                    return;
                }

                IGame newGame = PluginHelper.DataManager.AddNewGame(RomHackTitle);
                try
                {
                    AddRomHackLogMessage($"Creating {RomHackTitle}");

                    // metadata
                    newGame.Title = RomHackTitle;
                    newGame.LaunchBoxDbId = null;   // todo: look into how we can set the dbid
                    newGame.ReleaseDate = RomHackReleaseDate;
                    newGame.Rating = RomHackRating;
                    newGame.ReleaseType = RomHackReleaseType;
                    newGame.MaxPlayers = RomHackMaxPlayers;
                    newGame.GenresString = RomHackGenreString;
                    newGame.Platform = RomHackPlatform.PlatformName;
                    newGame.Developer = RomHackDeveloper;
                    newGame.Series = RomHackSeries;
                    newGame.Region = RomHackRegion;
                    newGame.PlayMode = RomHackPlayMode;
                    newGame.Version = RomHackVersion;
                    newGame.Status = RomHackStatus;
                    newGame.Source = RomHackSource;
                    newGame.DateAdded = DateTime.Now;
                    newGame.DateModified = DateTime.Now;
                    newGame.Favorite = RomHackFavorite;
                    newGame.Portable = RomHackPortable;
                    newGame.Completed = RomHackCompleted;
                    newGame.Hide = RomHackHide;
                    newGame.Broken = RomHackBroken;
                    newGame.Installed = RomHackInstalled;
                    newGame.VideoUrl = RomHackVideoUrl;
                    newGame.WikipediaUrl = RomHackWikipediaUrl;

                    // metadata - notes
                    newGame.Notes = RomHackNotes;

                    // metadata - sort title
                    newGame.SortTitle = RomHackSortTitle;

                    // metadata - additional apps 

                    // todo: metadata - controller support 

                    // todo: media
                    // newGame.ManualPath = SelectedGame.ManualPath;
                    // newGame.MusicPath = SelectedGame.MusicPath;

                    // media - images 
                    // media - videos

                    // launching 
                    newGame.ApplicationPath = patchedRomFilePath;

                    // newGame.CommandLine = SelectedGame.CommandLine;
                    // newGame.ConfigurationPath = SelectedGame.ConfigurationPath;
                    // newGame.ConfigurationCommandLine = SelectedGame.ConfigurationCommandLine;

                    // todo: launching - emulation
                    newGame.EmulatorId = SelectedGame.EmulatorId;

                    // todo: launching - startup/pause 
                    newGame.OverrideDefaultStartupScreenSettings = SelectedGame.OverrideDefaultStartupScreenSettings;
                    newGame.UseStartupScreen = selectedGame.UseStartupScreen;
                    newGame.DisableShutdownScreen = SelectedGame.DisableShutdownScreen;
                    newGame.HideMouseCursorInGame = SelectedGame.HideMouseCursorInGame;
                    newGame.AggressiveWindowHiding = SelectedGame.AggressiveWindowHiding;
                    newGame.HideAllNonExclusiveFullscreenWindows = SelectedGame.HideAllNonExclusiveFullscreenWindows;
                    newGame.StartupLoadDelay = selectedGame.StartupLoadDelay;
                }
                catch (Exception ex)
                {
                    AddRomHackLogMessage($"An unexpected error occurred while creating the rom hack in the local LaunchBox database: {ex.Message}");
                    LogHelper.LogException(ex, $"Creating launchbox game for {SelectedPatchFilePath}");
                    return;
                }

                string cleanTitle = DirectoryInfoHelper.GetCleanFileName(RomHackTitle);
                string fmt = "00";

                string videoFolder = string.Empty;

                IPlatform[] platforms = PluginHelper.DataManager.GetAllPlatforms();
                IPlatform platform = platforms.FirstOrDefault(p => p.Name == RomHackPlatform.PlatformName);
                if (platform != null)
                {
                    IPlatformFolder[] platformFolders = platform.GetAllPlatformFolders();

                    foreach (IPlatformFolder platformFolder in platformFolders)
                    {
                        IEnumerable<ImageToCopy> imageQuery = ImagesToCopy.Where(itc => itc.Copy && itc.ImageDetails.ImageType == platformFolder.MediaType);

                        string folderPath = Path.IsPathFullyQualified(platformFolder.FolderPath)
                            ? platformFolder.FolderPath
                            : Path.Combine(DirectoryInfoHelper.Instance.ApplicationPath, platformFolder.FolderPath);

                        if (platformFolder.MediaType.Equals("video", StringComparison.InvariantCultureIgnoreCase))
                        {
                            videoFolder = folderPath;
                        }

                        if (imageQuery.Any())
                        {
                            AddRomHackLogMessage($"Platform folder: {platformFolder.MediaType}, {Path.IsPathFullyQualified(platformFolder.FolderPath)} , {platformFolder.FolderPath}");
                            AddRomHackLogMessage($"Folder path: {folderPath}");
                        }

                        int imageTypeCount = 0;
                        foreach (ImageToCopy imageToCopy in imageQuery)
                        {
                            AddRomHackLogMessage($"Image: {imageToCopy.ImageDetails.ImageType}, {imageToCopy.ImageDetails.Region}, {imageToCopy.ImageDetails.FilePath}");

                            imageTypeCount++;

                            string imagePath = folderPath;
                            DirectoryInfoHelper.CreateDirectoryIfNotExists(imagePath);

                            if (!string.IsNullOrWhiteSpace(imageToCopy.ImageDetails.Region))
                            {
                                imagePath = Path.Combine(imagePath, imageToCopy.ImageDetails.Region);
                                DirectoryInfoHelper.CreateDirectoryIfNotExists(imagePath);
                            }

                            imagePath = Path.Combine(imagePath, cleanTitle + "-" + imageTypeCount.ToString(fmt) + Path.GetExtension(imageToCopy.ImageDetails.FilePath));
                            DirectoryInfoHelper.CreateDirectoryIfNotExists(imagePath);

                            AddRomHackLogMessage($"Destination path: {imagePath}");

                            if (!File.Exists(imagePath))
                            {
                                File.Copy(imageToCopy.ImageDetails.FilePath, imagePath);
                            }
                        }
                    }
                }

                // copy videos 
                if (!string.IsNullOrWhiteSpace(videoFolder))
                {
                    IEnumerable<VideoToCopy> videoQuery = VideosToCopy.Where(v => v.Copy).OrderBy(v => v.VideoType);

                    string videoType = string.Empty;
                    int videoTypeCount = 0;

                    foreach (VideoToCopy videoToCopy in videoQuery)
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
                        AddRomHackLogMessage($"Video: {videoToCopy.VideoType}, {videoToCopy.VideoPath}");

                        string videoPath = videoFolder;
                        DirectoryInfoHelper.CreateDirectoryIfNotExists(videoPath);

                        string mediaTypeFolder = GetVideoFolderForMediaType(videoToCopy.VideoType);

                        // add the video type to the path
                        if (!string.IsNullOrWhiteSpace(mediaTypeFolder))
                        {
                            videoPath = Path.Combine(videoPath, mediaTypeFolder);
                            DirectoryInfoHelper.CreateDirectoryIfNotExists(videoPath);
                        }

                        videoPath = Path.Combine(videoPath, cleanTitle + "-" + videoTypeCount.ToString(fmt) + Path.GetExtension(videoToCopy.VideoPath));
                        DirectoryInfoHelper.CreateDirectoryIfNotExists(videoPath);

                        AddRomHackLogMessage($"Destination video path: {videoPath}");

                        LogHelper.Log($"Video to copy: {videoToCopy.VideoType}, {videoToCopy.VideoPath}, {videoPath}");

                        if (!File.Exists(videoPath))
                        {
                            File.Copy(videoToCopy.VideoPath, videoPath);
                        }
                    }
                }

                // add additional apps - similar to above for additional apps 
                IEnumerable<AdditionalAppToCopy> additionalAppsToCopyQuery = AdditionalAppsToCopy.Where(a => a.Copy == true);
                try
                {
                    foreach (AdditionalAppToCopy additionalAppToCopy in additionalAppsToCopyQuery)
                    {
                        AddRomHackLogMessage($"Copying additional app: {additionalAppToCopy.AdditionalApplication.Name}");

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
                catch (Exception ex)
                {
                    AddRomHackLogMessage($"An unexpected error occurred while copying additional applicatoins for {RomHackTitle}: {ex.Message}");
                    LogHelper.LogException(ex, $"Copying additional apps for {RomHackTitle}");
                    return;
                }

                // save data and refresh
                try
                {
                    AddRomHackLogMessage($"Saving {RomHackTitle}");
                    PluginHelper.DataManager.Save(true);
                }
                catch (Exception ex)
                {
                    AddRomHackLogMessage($"An unexpected error occurred while saving {RomHackTitle} to local LaunchBox database: {ex.Message}");
                    LogHelper.LogException(ex, $"Saving {RomHackTitle} to local LaunchBox database");
                    return;
                }

                try
                {
                    AddRomHackLogMessage($"Refreshing LaunchBox");
                    PluginHelper.DataManager.ForceReload();
                    PluginHelper.LaunchBoxMainViewModel.RefreshData();
                }
                catch (Exception ex)
                {
                    AddRomHackLogMessage($"An unexpected error occurred while refreshing LaunchBox: {ex.Message}");
                    LogHelper.LogException(ex, $"Refreshing launchbox");
                    return;
                }

                success = true;
            });

            // close 
            if (success)
            {
                eventAggregator.GetEvent<ImportRomHackCancel>().Publish();
            }
        }

        private void AddRomHackLogMessage(string message)
        {
            RomHackLog = message + "\n" + RomHackLog;
        }

        private string GetVideoFolderForMediaType(string mediaType)
        {
            string returnValue;

            switch (mediaType)
            {
                case "Recording":
                    returnValue = "Recordings";
                    break;

                case "Theme Video":
                    returnValue = "Theme";
                    break;

                case "Trailer":
                    returnValue = "Trailer";
                    break;

                case "Marquee":
                    returnValue = "Marquee";
                    break;

                default:
                    returnValue = string.Empty;
                    break;
            }

            return returnValue;
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
                AddRomHackLogMessage($"Cleaning rom hack title: {RomHackTitle}");

                cleanRomHackTitle = DirectoryInfoHelper.GetCleanFileName(RomHackTitle);

                AddRomHackLogMessage($"Cleaned rom hack title: {cleanRomHackTitle}");
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while cleaning the rom hack title: {ex.Message}");
                LogHelper.LogException(ex, $"Cleaning rom hack title for {RomHackTitle}");
                return string.Empty;
            }

            // create temp dir in ..\plugins\this plugin\temp\guid
            string tempDirectory;
            try
            {
                AddRomHackLogMessage($"Creating temporary directory");
                tempDirectory = CreateTempDirectory();
                AddRomHackLogMessage($"Temporary directory created: {tempDirectory}");
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while creating the temporary directory: {ex.Message}");
                LogHelper.LogException(ex, $"Creating temporary directory for {RomHackTitle}");
                return string.Empty;
            }

            // make a copy of the source rom in a temp directory 
            string romFullPathInTempDirectory;
            try
            {
                AddRomHackLogMessage($"Copying source rom file to {tempDirectory}");
                romFullPathInTempDirectory = CopyOriginalRomToTemp(tempDirectory);
                AddRomHackLogMessage($"Copied source rom to temporary directory: {romFullPathInTempDirectory}");
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while copying the source rom to the temporary directory: {ex.Message}");
                LogHelper.LogException(ex, $"Copying source rom to temporary directory for {RomHackTitle}");
                return string.Empty;
            }

            // check if the source rom is zipped 
            bool isZipped = false;
            try
            {
                AddRomHackLogMessage($"Checking if source rom is a zip file: {romFullPathInTempDirectory}");
                if (string.Compare(Path.GetExtension(romFullPathInTempDirectory), ".zip", true) == 0)
                {
                    isZipped = true;
                }
                string tempMessage = isZipped ? "Source rom is a zip file" : "Source rom is not a zip file";
                AddRomHackLogMessage($"{tempMessage}");
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while checking whether the source rom is a zip file: {ex.Message}");
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

                    AddRomHackLogMessage($"Creating directory to unzip source rom: {tempZipDirectory}");

                    if (!Directory.Exists(tempZipDirectory))
                    {
                        Directory.CreateDirectory(tempZipDirectory);
                    }
                }
                catch (Exception ex)
                {
                    AddRomHackLogMessage($"An unexpected error occurred while creating a directory to unzip the source rom file: {ex.Message}");
                    LogHelper.LogException(ex, $"Attempting to create directory to unzip source rom: {tempZipDirectory}");
                    return string.Empty;
                }
            }

            // unzip source rom if it's a zip
            if (isZipped)
            {
                try
                {
                    AddRomHackLogMessage($"Unzipping {romFullPathInTempDirectory} to {tempZipDirectory}");
                    ZipFile.ExtractToDirectory(romFullPathInTempDirectory, tempZipDirectory);
                }
                catch (Exception ex)
                {
                    AddRomHackLogMessage($"An unexpected error occurred while trying to unzip {romFullPathInTempDirectory} to {tempZipDirectory}: {ex.Message}");
                    LogHelper.LogException(ex, $"Attempting to extract {romFullPathInTempDirectory} to {tempZipDirectory}");
                    return string.Empty;
                }
            }

            // get a handle on the extracted ROM file 
            if (isZipped)
            {
                AddRomHackLogMessage($"Getting a handle on the extracted ROM file in {tempZipDirectory}");
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
                    AddRomHackLogMessage($"An unexpected error occurred while trying to get a handle on the extracted rom file: {ex.Message}");
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
                AddRomHackLogMessage($"Renaming {romFullPathInTempDirectory} to {romCopyNewName}");

                // rename the rom to the patch name
                File.Move(romFullPathInTempDirectory, romCopyNewName);
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while attempting to rename {romFullPathInTempDirectory} to {romCopyNewName}: {ex.Message}");
                LogHelper.LogException(ex, $"Attempting to rename the source rom from {romFullPathInTempDirectory} to {romCopyNewName}");
                return string.Empty;
            }

            // copy the Rom Patch into the temp directory
            string romPatchCopyFileName = Path.GetFileName(SelectedPatchFilePath);
            string romPatchCopyDestinationFullPath = Path.Combine(tempDirectory, romPatchCopyFileName);
            try
            {
                AddRomHackLogMessage($"Copying rom patch from {SelectedPatchFilePath} to {romPatchCopyDestinationFullPath}");
                File.Copy(SelectedPatchFilePath, romPatchCopyDestinationFullPath, true);
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while attempting to copy the rom patch from {SelectedPatchFilePath} to {romPatchCopyDestinationFullPath}: {ex.Message}");
                LogHelper.LogException(ex, $"Attempting to copy {SelectedPatchFilePath} to {romPatchCopyDestinationFullPath}");
                return string.Empty;
            }

            // get command line arguments and replace the actual rom and patch paths 
            string commandLineArgs = SelectedPatcher.CommandLine;
            try
            {
                commandLineArgs = commandLineArgs.Replace("{patch}", $"\"{romPatchCopyDestinationFullPath}\"");
                commandLineArgs = commandLineArgs.Replace("{rom}", $"\"{romCopyNewName}\"");

                AddRomHackLogMessage($"Command line argument: {commandLineArgs}");
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while attempting create command line arguments for {commandLineArgs}: {ex.Message}");
                LogHelper.LogException(ex, $"Attempting to create command line arguments");
                return string.Empty;
            }

            // apply patch 
            try
            {
                AddRomHackLogMessage($"Creating rom patch using {SelectedPatcher.Path} and {commandLineArgs}\n{ RomHackLog}");

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

                AddRomHackLogMessage($"Rom patcher output: {processOutput}");

                if (!string.IsNullOrWhiteSpace(processError))
                {
                    throw new Exception($"Patch process returned the following error: {processError}");
                }
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while attempting to create the rom patch using {SelectedPatcher.Path} and arguments {commandLineArgs}: {ex.Message}");
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

                    AddRomHackLogMessage($"Zipping {tempZipDirectory} to {patchedRomFilePath}");

                    ZipFile.CreateFromDirectory(tempZipDirectory, patchedRomFilePath);
                }
                catch (Exception ex)
                {
                    AddRomHackLogMessage($"An unexpected error occurred while attempting to create zip file from {tempZipDirectory} to {patchedRomFilePath}: {ex.Message}");
                    LogHelper.LogException(ex, $"Attempting to zip {tempZipDirectory} to {patchedRomFilePath}");
                    return string.Empty;
                }
            }

            AddRomHackLogMessage($"Patched rom file: {patchedRomFilePath}");

            // create plug-in folder if it doesn't already exist 
            try
            {
                if (!Directory.Exists(DirectoryInfoHelper.Instance.PluginFolder))
                {
                    AddRomHackLogMessage($"Creating plugin folder: {DirectoryInfoHelper.Instance.PluginFolder}");
                    Directory.CreateDirectory(DirectoryInfoHelper.Instance.PluginFolder);
                }
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while attempting to check/create folder {DirectoryInfoHelper.Instance.PluginFolder}: {ex.Message}");
                LogHelper.LogException(ex, $"Attempting to check/create folder {DirectoryInfoHelper.Instance.PluginFolder}");
                return string.Empty;
            }

            // create ../plugin/rom hacks if it doesn't exist
            string pluginRomHacksDirectory = Path.Combine(DirectoryInfoHelper.Instance.PluginFolder, "Rom Hacks");
            try
            {
                if (!Directory.Exists(pluginRomHacksDirectory))
                {
                    AddRomHackLogMessage($"Creating plugin rom hacks folder: {pluginRomHacksDirectory}");
                    Directory.CreateDirectory(pluginRomHacksDirectory);
                }
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while attempting to check/create folder {pluginRomHacksDirectory}: {ex.Message}");
                LogHelper.LogException(ex, $"Attempting to check/create folder {pluginRomHacksDirectory}");
                return string.Empty;
            }

            // create ../plugin/rom hacks/{rom hack platform} if it doesn't exist
            string pluginRomHacksPlatformDirectory = Path.Combine(pluginRomHacksDirectory, RomHackPlatform.PlatformName);
            try
            {
                if (!Directory.Exists(pluginRomHacksPlatformDirectory))
                {
                    AddRomHackLogMessage($"Creating plugin rom hacks platform folder: {pluginRomHacksPlatformDirectory}");
                    Directory.CreateDirectory(pluginRomHacksPlatformDirectory);
                }
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while attempting to check/create folder {pluginRomHacksPlatformDirectory}: {ex.Message}");
                LogHelper.LogException(ex, $"Attempting to check/create folder {pluginRomHacksPlatformDirectory}");
                return string.Empty;
            }

            // create ../plugin/rom hacks/{rom hack platform}/{rom hack title} if it doesn't exist 
            string pluginRomHacksPlatformGameDirectory = Path.Combine(pluginRomHacksPlatformDirectory, cleanRomHackTitle);
            try
            {
                if (!Directory.Exists(pluginRomHacksPlatformGameDirectory))
                {
                    AddRomHackLogMessage($"Creating plugin rom hacks game folder {pluginRomHacksPlatformGameDirectory}");
                    Directory.CreateDirectory(pluginRomHacksPlatformGameDirectory);
                }
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while attempting to check/create folder {pluginRomHacksPlatformGameDirectory}: {ex.Message}");
                LogHelper.LogException(ex, $"Attempting to check/create folder {pluginRomHacksPlatformGameDirectory}");
                return string.Empty;
            }

            // fix file and folder attributes 
            try
            {
                AddRomHackLogMessage($"Fixing file attributes on {tempDirectory}");
                DirectoryInfoHelper.FixDirectoryAttributes(new DirectoryInfo(tempDirectory));
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while attempting to fix directory attributes on {tempDirectory}: {ex.Message}");
                LogHelper.LogException(ex, $"Attempting to fix attributes on folder {tempDirectory}");
                return string.Empty;
            }

            // move patch files to plugin/patches/{platform}/{rom hack title} ? 

            // copy the patch to the game directory
            string romPatchFinalDestinationFileName = Path.Combine(pluginRomHacksPlatformGameDirectory, romPatchCopyFileName);
            try
            {
                AddRomHackLogMessage($"Copying patch to plugin rom hacks game folder {romPatchCopyDestinationFullPath} to {romPatchFinalDestinationFileName}");

                File.Copy(romPatchCopyDestinationFullPath, romPatchFinalDestinationFileName, true);
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while attempting to copy rom patch to game folder from {romPatchCopyDestinationFullPath} to {romPatchFinalDestinationFileName}: {ex.Message}");
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
                AddRomHackLogMessage($"An unexpected error occurred while attempting to copy patched rom to game folder from {patchedRomFilePath} to {patchedGameFinalDestinationFileName}: {ex.Message}");
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
                AddRomHackLogMessage($"An unexpected error occurred while attempting to delete the temp directory {DirectoryInfoHelper.Instance.PluginTempFolder}: {ex.Message}");
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

        public DateTime? RomHackReleaseDate
        {
            get { return romHackReleaseDate; }
            set
            {
                romHackReleaseDate = value;
                OnPropertyChanged("RomhackReleaseDate");
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

        public string RomHackLog
        {
            get { return romHackLog; }
            set
            {
                romHackLog = value;
                OnPropertyChanged("RomHackLog");
            }
        }

        public string RomHackRating
        {
            get { return romHackRating; }
            set
            {
                romHackRating = value;
                OnPropertyChanged("RomHackRating");
            }
        }

        public string RomHackReleaseType
        {
            get { return romHackReleaseType; }
            set
            {
                romHackReleaseType = value;
                OnPropertyChanged("RomHackReleaseType");
            }
        }
        public int? RomHackMaxPlayers
        {
            get { return romHackMaxPlayers; }
            set
            {
                romHackMaxPlayers = value;
                OnPropertyChanged("RomHackMaxPlayers");
            }
        }

        public string RomHackGenreString
        {
            get { return romHackGenreString; }
            set
            {
                romHackGenreString = value;
                OnPropertyChanged("RomHackGenreString");
            }
        }

        public string RomHackDeveloper
        {
            get { return romHackDeveloper; }
            set
            {
                romHackDeveloper = value;
                OnPropertyChanged("RomHackDeveloper");
            }
        }
        public string RomHackSeries
        {
            get { return romHackSeries; }
            set
            {
                romHackSeries = value;
                OnPropertyChanged("RomHackSeries");
            }
        }

        public string RomHackRegion
        {
            get { return romHackRegion; }
            set
            {
                romHackRegion = value;
                OnPropertyChanged("RomHackRegion");
            }
        }

        public string RomHackVersion
        {
            get { return romHackVersion; }
            set
            {
                romHackVersion = value;
                OnPropertyChanged("RomHackVersion");
            }
        }

        public string RomHackPlayMode
        {
            get { return romHackPlayMode; }
            set
            {
                romHackPlayMode = value;
                OnPropertyChanged("RomHackPlayMode");
            }
        }

        public string RomHackPublisher
        {
            get { return romHackPublisher; }
            set
            {
                romHackPublisher = value;
                OnPropertyChanged("RomHackPublisher");
            }
        }

        public string RomHackSource
        {
            get { return romHackSource; }
            set
            {
                romHackSource = value;
                OnPropertyChanged("RomHackSource");
            }
        }

        public string RomHackStatus
        {
            get { return romHackStatus; }
            set
            {
                romHackStatus = value;
                OnPropertyChanged("RomHackStatus");
            }
        }

        public DateTime? RomHackDateAdded
        {
            get { return romHackDateAdded; }
            set
            {
                romHackDateAdded = value;
                OnPropertyChanged("RomHackDateAdded");
            }
        }

        public string RomHackWikipediaUrl
        {
            get { return romHackWikipediaUrl; }
            set
            {
                romHackWikipediaUrl = value;
                OnPropertyChanged("RomHackWikipediaUrl");
            }
        }

        public string RomHackVideoUrl
        {
            get { return romHackVideoUrl; }
            set
            {
                romHackVideoUrl = value;
                OnPropertyChanged("RomHackVideoUrl");
            }
        }

        public float RomHackStarRating
        {
            get { return romHackStarRating; }
            set
            {
                romHackStarRating = value;
                OnPropertyChanged("RomHackStarRating");
            }
        }

        public int RomHackPlayCount
        {
            get { return romHackPlayCount; }
            set
            {
                romHackPlayCount = value;
                OnPropertyChanged("RomHackPlayCount");
            }
        }

        public DateTime? RomHackLastPlayedDate
        {
            get { return romHackLastPlayedDate; }
            set
            {
                romHackLastPlayedDate = value;
                OnPropertyChanged("RomHackLastPlayedDate");
            }
        }

        public string SelectedTabPage
        {
            get { return selectedTabPage; }
            set
            {
                selectedTabPage = value;
                OnPropertyChanged("SelectedTabPage");

                UpdateTabVisibility();
            }
        }

        private void UpdateTabVisibility()
        {
            RomHackVisibility = Visibility.Collapsed;
            MetadataVisibility = Visibility.Collapsed;
            NotesVisibility = Visibility.Collapsed;
            AdditionalAppsVisibility = Visibility.Collapsed;
            MediaVisibility = Visibility.Collapsed;
            ImagesVisibility = Visibility.Collapsed;
            VideosVisibility = Visibility.Collapsed;
            EmulationVisibility = Visibility.Collapsed;
            StartupPauseVisibility = Visibility.Collapsed;
            LogVisibility = Visibility.Collapsed;

            switch (SelectedTabPage)
            {
                case "Rom Hack":
                    RomHackVisibility = Visibility.Visible;
                    break;

                case "Metadata":
                    MetadataVisibility = Visibility.Visible;
                    break;

                case "Notes":
                    NotesVisibility = Visibility.Visible;
                    break;

                case "Additional Apps":
                    AdditionalAppsVisibility = Visibility.Visible;
                    break;

                case "Media":
                    MediaVisibility = Visibility.Visible;
                    break;

                case "Images":
                    ImagesVisibility = Visibility.Visible;
                    break;

                case "Videos":
                    VideosVisibility = Visibility.Visible;
                    break;

                case "Emulation":
                    EmulationVisibility = Visibility.Visible;
                    break;

                case "Startup/Pause":
                    StartupPauseVisibility = Visibility.Visible;
                    break;

                case "Log":
                    LogVisibility = Visibility.Visible;
                    break;
            }
        }


        public Visibility MetadataVisibility
        {
            get { return metadataVisibility; }
            set
            {
                metadataVisibility = value;
                OnPropertyChanged("MetadataVisibility");
            }
        }

        public Visibility NotesVisibility
        {
            get { return notesVisibility; }
            set
            {
                notesVisibility = value;
                OnPropertyChanged("NotesVisibility");
            }
        }

        public Visibility LogVisibility
        {
            get { return logVisibility; }
            set
            {
                logVisibility = value;
                OnPropertyChanged("LogVisibility");
            }
        }

        public Visibility AdditionalAppsVisibility
        {
            get { return additionalAppsVisibility; }
            set
            {
                additionalAppsVisibility = value;
                OnPropertyChanged("AdditionalAppsVisibility");
            }
        }

        public Visibility MediaVisibility
        {
            get { return mediaVisibility; }
            set
            {
                mediaVisibility = value;
                OnPropertyChanged("MediaVisibility");
            }
        }

        public Visibility ImagesVisibility
        {
            get { return imagesVisibility; }
            set
            {
                imagesVisibility = value;
                OnPropertyChanged("ImagesVisibility");
            }
        }

        public Visibility VideosVisibility
        {
            get { return videosVisibility; }
            set
            {
                videosVisibility = value;
                OnPropertyChanged("VideosVisibility");
            }
        }

        public Visibility EmulationVisibility
        {
            get { return emulationVisibility; }
            set
            {
                emulationVisibility = value;
                OnPropertyChanged("EmulationVisibility");
            }
        }

        public Visibility StartupPauseVisibility
        {
            get { return startupPauseVisibility; }
            set
            {
                startupPauseVisibility = value;
                OnPropertyChanged("StartupPauseVisibility");
            }
        }

        public Visibility RomHackVisibility
        {
            get { return romHackVisibility; }
            set
            {
                romHackVisibility = value;
                OnPropertyChanged("RomHackVisibility");
            }
        }

        public string RomHackNotes
        {
            get { return romHackNotes; }
            set
            {
                romHackNotes = value;
                OnPropertyChanged("RomHackNotes");
            }
        }

        public string RomHackSortTitle
        {
            get { return romHackSortTitle; }
            set
            {
                romHackSortTitle = value;
                OnPropertyChanged("RomHackSortTitle");
            }
        }

        public bool RomHackFavorite
        {
            get { return romHackFavorite; }
            set
            {
                romHackFavorite = value;
                OnPropertyChanged("RomHackFavorite");
            }
        }

        public bool RomHackPortable
        {
            get { return romHackPortable; }
            set
            {
                romHackPortable = value;
                OnPropertyChanged("RomHackPortable");
            }
        }

        public bool RomHackCompleted
        {
            get { return romHackCompleted; }
            set
            {
                romHackCompleted = value;
                OnPropertyChanged("RomHackCompleted");
            }
        }

        public bool RomHackHide
        {
            get { return romHackHide; }
            set
            {
                romHackHide = value;
                OnPropertyChanged("RomHackHide");
            }
        }

        public bool RomHackBroken
        {
            get { return romHackBroken; }
            set
            {
                romHackBroken = value;
                OnPropertyChanged("RomHackBroken");
            }
        }

        public bool? RomHackInstalled
        {
            get { return romHackInstalled; }
            set
            {
                romHackInstalled = value;
                OnPropertyChanged("RomHackInstalled");
            }
        }

        public Uri IconUri { get; } = ResourceImages.RomHackingIconPath;
    }
}
