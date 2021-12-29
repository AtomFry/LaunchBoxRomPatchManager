using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace LaunchBoxRomPatchManager.Model
{
    [XmlRoot("LaunchBox")]
    public class LaunchBox
    {
        public LaunchBox()
        {
            Games = new List<MetadataGame>();
        }

        [XmlElement("Game")]
        public List<MetadataGame> Games { get; set; }
    }

    public class MetadataGame
    {
        public string Name { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string ReleaseType { get; set; }
        public int DatabaseID { get; set; }
        public string Platform { get; set; }
        public string Genres { get; set; }
        public int? ReleaseYear { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string Overview { get; set; }
        public int? MaxPlayers { get; set; }
        public string VideoURL { get; set; }
        public string WikipediaURL { get; set; }
    }


    public class SearchHelper
    {
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Verify arguments.
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Initialize arrays.
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Begin looping.
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    // Compute cost.
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
                }
            }
            // Return cost.
            return d[n, m];
        }
    }
}
