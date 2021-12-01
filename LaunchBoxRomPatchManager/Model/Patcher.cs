using System.Collections.Generic;

namespace LaunchBoxRomPatchManager.Model
{
    public class Patcher
    {
        public Patcher()
        {
            Platforms = new List<PatcherPlatform>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string CommandLine { get; set; }
        public List<PatcherPlatform> Platforms { get; set; }
    }
}
