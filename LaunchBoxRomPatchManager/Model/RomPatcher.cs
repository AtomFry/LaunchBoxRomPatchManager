using LaunchBoxRomPatchManager.ModelWrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unbroken.LaunchBox.Plugins.Data;

namespace LaunchBoxRomPatchManager.Model
{
    [Obsolete]
    public class RomPatcher
    {
        public RomPatcher()
        {
            Platforms = new List<RomPatcherPlatform>();            
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string FilePath { get; set; }

        public string CommandLine { get; set; }

        public List<RomPatcherPlatform> Platforms { get; set; }        
    }
}
