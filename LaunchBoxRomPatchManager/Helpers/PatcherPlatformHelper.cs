using System;
using System.Collections.Generic;
using System.Text;
using Unbroken.LaunchBox.Plugins.Data;

namespace LaunchBoxRomPatchManager.Helpers
{
    public class PatcherPlatformHelper
    {
        public static List<string> GetDefaultIpsPlatforms()
        {
            List<string> defaultIpsPlatforms = new List<string>();

            IPlatform[] allPlatforms = Unbroken.LaunchBox.Plugins.PluginHelper.DataManager.GetAllPlatforms();
            foreach(IPlatform platform in allPlatforms)
            {
                if (IsDefaultIpsPlatform(platform.Name))
                {
                    defaultIpsPlatforms.Add(platform.Name);
                }
            }

            return defaultIpsPlatforms;
        }

        public static List<string> GetDefaultPpfPlatforms()
        {
            List<string> defaultPpfPlatforms = new List<string>();

            IPlatform[] allPlatforms = Unbroken.LaunchBox.Plugins.PluginHelper.DataManager.GetAllPlatforms();
            foreach (IPlatform platform in allPlatforms)
            {
                if (IsDefaultPpfPlatform(platform.Name))
                {
                    defaultPpfPlatforms.Add(platform.Name);
                }
            }

            return defaultPpfPlatforms;
        }

        private static bool IsDefaultPpfPlatform(string platform)
        {
            if (string.IsNullOrWhiteSpace(platform)) return false;

            bool isDefaultPpfPlatform = false;
            switch (platform.ToLower())
            {
                case "sony playstation":
                    isDefaultPpfPlatform = true;
                    break;

                case "sony psp":
                    isDefaultPpfPlatform = true;
                    break;

                default:
                    isDefaultPpfPlatform = false;
                    break;
            }
            return isDefaultPpfPlatform;

        }

        public static bool IsDefaultIpsPlatform(string platform)
        {
            if (string.IsNullOrWhiteSpace(platform)) return false;

            bool isDefaultIpsPlatform = false;
            switch (platform.ToLower())
            {
                case "nintendo entertainment system": 
                    isDefaultIpsPlatform = true;
                    break;

                case "nintendo game boy advance":
                    isDefaultIpsPlatform = true;
                    break;

                case "nintendo game boy color":
                    isDefaultIpsPlatform = true;
                    break;

                case "nintendo 64":
                    isDefaultIpsPlatform = true;
                    break;

                case "super nintendo entertainment system":
                    isDefaultIpsPlatform = true;
                    break;

                case "sega genesis":
                    isDefaultIpsPlatform = true;
                    break;

                case "sega 32x":
                    isDefaultIpsPlatform = true;
                    break;

                case "nec turbografx-16":
                    isDefaultIpsPlatform = true;
                    break;

                default:
                    isDefaultIpsPlatform = false;
                    break;
            }
            return isDefaultIpsPlatform;
        }
    }
}
