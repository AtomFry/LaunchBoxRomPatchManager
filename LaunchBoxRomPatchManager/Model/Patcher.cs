using LaunchBoxRomPatchManager.Helpers;
using System.Collections.Generic;

namespace LaunchBoxRomPatchManager.Model
{
    public class Patcher
    {
        public Patcher()
        {
            Platforms = new List<string>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string CommandLine { get; set; }
        public List<string> Platforms { get; set; }

        public string FullPath
        {
            get
            {
                return System.IO.Path.IsPathFullyQualified(Path)
                    ? Path
                    : System.IO.Path.Combine(DirectoryInfoHelper.Instance.ApplicationPath, Path);
            }
        }
    }
}
