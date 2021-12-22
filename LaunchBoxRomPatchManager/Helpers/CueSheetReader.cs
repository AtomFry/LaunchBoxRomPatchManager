using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LaunchBoxRomPatchManager.Helpers
{
    public class CueSheetReader
    {
        public static void ReplaceTextInCueFile(string cueSheetPath, string originalValue, string newValue)
        {
            string strFileContent = File.ReadAllText(cueSheetPath);
            strFileContent = strFileContent.Replace(originalValue, newValue);
            File.WriteAllText(cueSheetPath, strFileContent);
        }

        public static List<string> ReadCueSheet(string cueSheetPath, bool includeCueSheet = true)
        {
            List<string> cueSheetFiles = new List<string>();

            if(includeCueSheet)
            {
                cueSheetFiles.Add(cueSheetPath);
            }

            using (FileStream fs = new FileStream(cueSheetPath, FileMode.Open, FileAccess.Read, FileShare.Read, 2048, FileOptions.SequentialScan))
            using (TextReader source = new StreamReader(fs, Encoding.UTF8))
            {
                string s = source.ReadLine();
                string referencedFilePath = "";
                while (s != null)
                {
                    s = s.Trim();

                    int firstBlank = s.IndexOf(' ');
                    string firstWord = s.Substring(0, firstBlank);

                    if ("FILE".Equals(firstWord, StringComparison.OrdinalIgnoreCase))
                    {
                        referencedFilePath = s.Substring(firstBlank + 1, s.Length - firstBlank - 1);
                        referencedFilePath = referencedFilePath.Substring(0, referencedFilePath.LastIndexOf(' ')); // Get rid of the last word representing the audio format
                        referencedFilePath = stripBeginEndQuotes(referencedFilePath);

                        // Strip the ending word representing the audio format
                        if (!Path.IsPathRooted(referencedFilePath))
                        {
                            referencedFilePath = Path.Combine(Path.GetDirectoryName(cueSheetPath), referencedFilePath);
                        }

                        cueSheetFiles.Add(referencedFilePath);
                    }

                    s = source.ReadLine();
                }
            }
            return cueSheetFiles;
        }

        private static string stripBeginEndQuotes(string s)
        {
            if (s.Length < 2) return s;
            if ((s[0] != '"') || (s[s.Length - 1] != '"')) return s;

            return s.Substring(1, s.Length - 2);
        }
    }
}
