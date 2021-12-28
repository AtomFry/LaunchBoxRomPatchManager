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
}
