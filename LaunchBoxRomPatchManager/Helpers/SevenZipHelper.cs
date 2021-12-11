using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    }
}
