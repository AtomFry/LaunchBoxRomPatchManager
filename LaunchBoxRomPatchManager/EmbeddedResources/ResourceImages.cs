using LaunchBoxRomPatchManager.Helpers;
using System;

namespace LaunchBoxRomPatchManager.EmbeddedResources
{
    public class ResourceImages
    {
        public static Uri RomHackingIconPath { get; } = new Uri($"{DirectoryInfoHelper.ResourceFolder}/RomHackingIcon.png");
    }
}
