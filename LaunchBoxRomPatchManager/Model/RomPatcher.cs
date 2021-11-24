using LaunchBoxRomPatchManager.ModelWrapper;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unbroken.LaunchBox.Plugins.Data;

namespace LaunchBoxRomPatchManager.Model
{
    public class RomPatcher
    {
        public RomPatcher()
        {
            Platforms = new List<RomPatcherPlatform>();
            Emulators = new List<RomPatcherEmulator>();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string FilePath { get; set; }

        public string CommandLine { get; set; }

        public List<RomPatcherPlatform> Platforms { get; set; }
        public List<RomPatcherEmulator> Emulators { get; set; }
    }
}
