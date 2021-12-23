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
        public ObservableCollection<IPlatform> PlatformLookup { get; }
        public ObservableCollection<string> TabPages { get; }
        public ObservableCollection<SourceFile> SourceRomFiles { get; }
        public ObservableCollection<SourceFile> SourcePatchFiles { get; }

        private IGame selectedGame;
        private ImageToCopy selectedImageToCopy;
        private VideoToCopy selectedVideoToCopy;
        private AdditionalAppToCopy selectedAdditionalAppToCopy;
        private string selectedTabPage;
        private Patcher selectedPatcher;
        private string selectedPatchFilePath;
        private string workingDirectory;
        private bool sourceFileWasZipped;
        private bool sourceFileWasCue;
        private string sourceCueFile;
        private string romFolderInWorkingDirectory;
        private SourceFile selectedSourceRomFile;
        private SourceFile selectedSourcePatchFile;

        private IPlatform romHackPlatform;
        private string romHackTitle;
        private string cleanRomHackTitle;
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
            PlatformLookup = new ObservableCollection<IPlatform>();
            SourceRomFiles = new ObservableCollection<SourceFile>();
            SourcePatchFiles = new ObservableCollection<SourceFile>();

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
            // creates a temp folder to work out of
            InitializeWorkingDirectory();

            // create the available tabs
            InitializeTabPages();

            // initialize the patcher to use from the selected game
            await InitializePatcherAsync();

            // populate the platform lookup
            InitializePlatformLookup();

            // initialize source rom files from selected game's application path 
            InitializeSourceRomFiles();

            // initialize rom hack properties from selected game
            InitializeRomHackProperties();
        }

        private void InitializeWorkingDirectory()
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

            workingDirectory = tempDir;
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

        private async Task InitializePatcherAsync()
        {
            // get all patchers from the data file
            PatcherDataProvider patcherDataProvider = new PatcherDataProvider();
            IEnumerable<Patcher> patchers = await patcherDataProvider.GetAllPatchersAsync();

            // find the patcher that has this platform assigned
            foreach (Patcher patcher in patchers)
            {
                foreach (string platform in patcher.Platforms)
                {
                    if (platform == SelectedGame.Platform)
                    {
                        SelectedPatcher = patcher;
                        break;
                    }
                }
            }
        }

        private void InitializePlatformLookup()
        {
            IPlatform[] platforms = PluginHelper.DataManager.GetAllPlatforms().OrderBy(p => p.Name).ToArray();
            foreach (IPlatform platform in platforms)
            {
                PlatformLookup.Add(platform);
            }
        }

        private void InitializeRomHackProperties()
        {
            RomHackTitle = SelectedGame.Title;
            RomHackSortTitle = SelectedGame.SortTitle;
            RomHackPlatform = PlatformLookup.FirstOrDefault(p => p.Name == SelectedGame.Platform);
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

        private void InitializeSourceRomFiles()
        {
            // get the source rom files depending on the source file's extension 
            string sourceRomExtension = Path.GetExtension(SelectedGame.ApplicationPath);
            sourceRomExtension = sourceRomExtension.ToLower();

            // create a folder in the working directory for the source rom file(s)
            romFolderInWorkingDirectory = Path.Combine(workingDirectory, "ROM");
            if (!Directory.Exists(romFolderInWorkingDirectory))
            {
                Directory.CreateDirectory(romFolderInWorkingDirectory);
            }

            sourceFileWasZipped = false;
            switch (sourceRomExtension)
            {
                case ".7z":
                case ".rar":
                case ".zip":
                    sourceFileWasZipped = true;

                    // extract archive to populate source rom collection
                    SevenZipHelper.Extract(SelectedGame.ApplicationPath, romFolderInWorkingDirectory);
                    break;

                case ".cue":
                    sourceFileWasCue = true;

                    // read cue sheet to populate source rom collection with cue file and bin files
                    List<string> cueFiles = CueSheetReader.ReadCueSheet(SelectedGame.ApplicationPath);
                    foreach (string cueFile in cueFiles)
                    {
                        string cueFileDestination = Path.Combine(romFolderInWorkingDirectory, Path.GetFileName(cueFile));
                        File.Copy(cueFile, cueFileDestination);

                        if(cueFileDestination.ToLower().EndsWith(".cue"))
                        {
                            sourceCueFile = cueFileDestination;
                        }
                    }
                    break;

                default:
                    string destinationFile = Path.Combine(romFolderInWorkingDirectory, Path.GetFileName(SelectedGame.ApplicationPath));
                    File.Copy(SelectedGame.ApplicationPath, destinationFile);
                    break;
            }

            // add files to the SourceRomFiles collection and default a selected rom file
            IEnumerable<string> filesInDirectory = Directory.EnumerateFiles(romFolderInWorkingDirectory);
            foreach (string file in filesInDirectory)
            {
                SourceFile sourceFile = new SourceFile(file);
                SourceRomFiles.Add(sourceFile);

                if (SelectedSourceRomFile == null && DirectoryInfoHelper.IsRomFIle(file))
                {
                    SelectedSourceRomFile = sourceFile;
                }
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

            // todo: need to disable when rom hack creation is in progress

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
                    newGame.Platform = RomHackPlatform.Name;
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
                IPlatform platform = RomHackPlatform;
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

        private string CreateRomHack()
        {
            // get command line arguments and replace the actual rom and patch paths 
            string commandLineArgs = SelectedPatcher.CommandLine;
            try
            {
                commandLineArgs = commandLineArgs.Replace("{patch}", $"\"{SelectedSourcePatchFile.SourceFilePath}\"");
                commandLineArgs = commandLineArgs.Replace("{rom}", $"\"{SelectedSourceRomFile.SourceFilePath}\"");

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
                AddRomHackLogMessage($"Creating rom patch using {SelectedPatcher.FullPath} and {commandLineArgs}");

                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = SelectedPatcher.FullPath,
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


                if(string.IsNullOrWhiteSpace(processOutput))
                {
                    processOutput = "The patching process produced no output";
                }

                // todo: looks like the output from pdx-ppf is very long and causing the import to hang - trim this string?
                int outputlen = processOutput.Length;

                LogHelper.Log($"Output length: {outputlen}");

                if (outputlen > 300)
                {
                    outputlen = 300;
                }

                AddRomHackLogMessage($"Rom patcher output: {processOutput.Substring(0, outputlen)}");

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
            
            // if cue file - need to update the cue file references 
            if (sourceFileWasCue)
            {
                CueSheetReader.ReplaceTextInCueFile(sourceCueFile, Path.GetFileNameWithoutExtension(SelectedGame.ApplicationPath), CleanRomHackTitle);
            }            

            // rename files to new CleanRomHackTitle, replacing the selected application file name with the new clean rom hack title 
            string originalName = Path.GetFileNameWithoutExtension(SelectedGame.ApplicationPath);
            DirectoryInfoHelper.RenameFolderAndFiles(romFolderInWorkingDirectory, originalName, CleanRomHackTitle);

            // zip the rom folder if it was zipped 
            string zipFileName = string.Empty;
            if (sourceFileWasZipped)
            {
                zipFileName = Path.Combine(workingDirectory, CleanRomHackTitle + ".zip");
                ZipFile.CreateFromDirectory(romFolderInWorkingDirectory, zipFileName);
            }

            // fix file and folder attributes 
            try
            {
                AddRomHackLogMessage($"Fixing file attributes on {workingDirectory}");
                DirectoryInfoHelper.FixDirectoryAttributes(new DirectoryInfo(workingDirectory));
            }
            catch (Exception ex)
            {
                AddRomHackLogMessage($"An unexpected error occurred while attempting to fix directory attributes on {workingDirectory}: {ex.Message}");
                LogHelper.LogException(ex, $"Attempting to fix attributes on folder {workingDirectory}");
                return string.Empty;
            }

            // identify destination directory - LB platform has a games directory
            string finalDestinationFolder = string.IsNullOrWhiteSpace(RomHackPlatform.Folder)
                ? Path.Combine(DirectoryInfoHelper.Instance.LaunchboxGamesPath, RomHackPlatform.Name)
                : RomHackPlatform.Folder;

            string finalDestinationFile;
            if (sourceFileWasZipped)
            {
                // if zipped - move zip file to destination directory 
                finalDestinationFile = Path.Combine(finalDestinationFolder, Path.GetFileName(zipFileName));
                File.Copy(zipFileName, finalDestinationFile, true);
            }
            else
            {
                // if not zipped - move all files from the rom folder to the destination directory 
                DirectoryInfoHelper.DirectoryCopy(romFolderInWorkingDirectory, finalDestinationFolder, true);

                // get a handle on the new application file
                string originalFileExtension = Path.GetExtension(SelectedGame.ApplicationPath);
                finalDestinationFile = Path.Combine(finalDestinationFolder, CleanRomHackTitle + originalFileExtension);
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

            return finalDestinationFile;
        }

        private void OnSelectNoAdditionalAppsExecute()
        {
            SelectAllAdditionalApps(false);
        }

        private void OnSelectAllAdditionalAppsExecute()
        {
            SelectAllAdditionalApps(true);
        }

        private void SelectAllAdditionalApps(bool _copy)
        {
            AdditionalAppToCopy[] tempAdditionalAppTopCopy = AdditionalAppsToCopy.ToArray();

            AdditionalAppsToCopy.Clear();
            foreach (AdditionalAppToCopy additionalAppToCopy in tempAdditionalAppTopCopy)
            {
                AdditionalAppsToCopy.Add(new AdditionalAppToCopy()
                {
                    AdditionalApplication = additionalAppToCopy.AdditionalApplication,
                    Copy = _copy
                });
            }
        }

        private void SelectAllVideos(bool _copy)
        {
            VideoToCopy[] tempVideosToCopy = VideosToCopy.ToArray();

            VideosToCopy.Clear();
            foreach (VideoToCopy videoToCopy in tempVideosToCopy)
            {
                VideosToCopy.Add(new VideoToCopy()
                {
                    VideoType = videoToCopy.VideoType,
                    VideoPath = videoToCopy.VideoPath,
                    Copy = _copy
                });
            }
        }

        private void OnSelectNoVideosExecute()
        {
            SelectAllVideos(false);
        }

        private void OnSelectAllVideosExecute()
        {
            SelectAllVideos(true);
        }


        private void OnSelectNoImagesExecute()
        {
            SelectAllImages(false);
        }

        private void OnSelectAllImagesExecute()
        {
            SelectAllImages(true);
        }

        private void SelectAllImages(bool _copy)
        {
            ImageToCopy[] tempImageToCopy = ImagesToCopy.ToArray();

            ImagesToCopy.Clear();
            foreach (ImageToCopy imageToCopy in tempImageToCopy)
            {
                ImagesToCopy.Add(new ImageToCopy() { ImageDetails = imageToCopy.ImageDetails, Copy = _copy });
            }
        }

        private void OnSelectPatchFileExecute()
        {
            OpenFileDialog openFileDialog;
            openFileDialog = new OpenFileDialog();

            // todo: create system setting for default browse for patch file path 
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

                // create a "Patch" folder in the working directory and copy the selected patch file there 
                string patchFolderInWorkingDirectory = Path.Combine(workingDirectory, "Patch");

                // delete the patch folder so that we start with a fresh set of patch files 
                if (Directory.Exists(patchFolderInWorkingDirectory))
                {
                    Directory.Delete(patchFolderInWorkingDirectory, true);
                }

                // (re)create the folder 
                Directory.CreateDirectory(patchFolderInWorkingDirectory);

                if (!SevenZipHelper.IsArchiveFile(SelectedPatchFilePath))
                {
                    // if it's not an archive file then just copy the patch file into the inner patch folder path 
                    File.Copy(SelectedPatchFilePath, Path.Combine(patchFolderInWorkingDirectory, Path.GetFileName(SelectedPatchFilePath)));
                }
                else
                {
                    // extract the selected file to the unarchived patch folder in the working directory 
                    SevenZipHelper.Extract(SelectedPatchFilePath, patchFolderInWorkingDirectory);
                }

                SourcePatchFiles.Clear();

                IEnumerable<string> patchFiles = Directory.EnumerateFiles(patchFolderInWorkingDirectory, "*", SearchOption.AllDirectories);
                foreach (string patchFile in patchFiles)
                {
                    SourceFile sourceFile = new SourceFile(patchFile);
                    SourcePatchFiles.Add(sourceFile);
                    if (DirectoryInfoHelper.IsPatchFile(patchFile))
                    {
                        SelectedSourcePatchFile = sourceFile;
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

                // raise the event that determines if the create button is enabled
                InvalidateCommands();
            }
        }

        public string SelectedPatchFilePath
        {
            get { return selectedPatchFilePath; }
            set
            {
                selectedPatchFilePath = value;
                OnPropertyChanged("SelectedPatchFilePath");

                // raise the event that determines if the create button is enabled
                InvalidateCommands();
            }
        }

        public string RomHackTitle
        {
            get { return romHackTitle; }
            set
            {
                romHackTitle = value;
                OnPropertyChanged("RomHackTitle");

                // get the clean title 
                CleanRomHackTitle = DirectoryInfoHelper.GetCleanFileName(RomHackTitle);

                // raise the event that determines if the create button is enabled
                InvalidateCommands();
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

        public IPlatform RomHackPlatform
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

        public SourceFile SelectedSourceRomFile
        {
            get { return selectedSourceRomFile; }
            set
            {
                selectedSourceRomFile = value;
                OnPropertyChanged("SelectedSourceRomFile");
            }
        }

        public SourceFile SelectedSourcePatchFile
        {
            get { return selectedSourcePatchFile; }
            set
            {
                selectedSourcePatchFile = value;
                OnPropertyChanged("SelectedSourcePatchFile");
            }
        }

        public string CleanRomHackTitle
        {
            get { return cleanRomHackTitle; }
            set
            {
                cleanRomHackTitle = value;
                OnPropertyChanged("CleanRomHackTitle");
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

        public Uri IconUri { get; } = ResourceImages.RomHackingIconPath;

        private void InvalidateCommands()
        {
            // call this whenever a property changes that impacts whether a command should be able to execute
            ((DelegateCommand)CreateRomHackCommand).RaiseCanExecuteChanged();
        }

    }
}
