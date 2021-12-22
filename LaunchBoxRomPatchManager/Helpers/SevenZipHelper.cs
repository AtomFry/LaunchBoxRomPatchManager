using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LaunchBoxRomPatchManager.Helpers
{
    public class SevenZipHelper
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }

        private void TryExtract(string archiveFile, string destination)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.FileName = DirectoryInfoHelper.Instance.SevenZipPath;
                processStartInfo.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", archiveFile, destination);

                using (Process process = Process.Start(processStartInfo))
                {
                    process.WaitForExit();
                    IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                Message = ex.Message;
            }
        }

        public static SevenZipHelper Extract(string archiveFile, string destination)
        {
            SevenZipHelper sevenZipHelper = new SevenZipHelper();
            sevenZipHelper.TryExtract(archiveFile, destination);
            return sevenZipHelper;
        }

        public static bool IsArchiveFile(string fileName)
        {
            bool isArchiveFile = false;

            string extension = Path.GetExtension(fileName);
            extension = extension.ToLower();

            switch(extension)
            {
                case ".7z":
                case ".zip":
                case ".rar":
                    isArchiveFile = true;
                    break;

                default:
                    break;
            }

            return isArchiveFile;
        }
    }
}
