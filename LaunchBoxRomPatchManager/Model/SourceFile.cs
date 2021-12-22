using System.IO;

namespace LaunchBoxRomPatchManager.Model
{
    public class SourceFile
    {
        public string SourceFilePath { get; set; }
        public string SourceFileName { get; set; }
        public string SourceFileExtension { get; set; }

        public SourceFile(string filePath)
        {
            SourceFilePath = filePath;
            SourceFileName = Path.GetFileName(filePath);
            SourceFileExtension = Path.GetExtension(filePath);
        }
    }
}
