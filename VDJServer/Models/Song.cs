using System.Collections.Generic;

namespace VDJServer.Models
{
    public class Song
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Album { get; set; }

        public string LeadAuthor { get; set; }

        public string FeatureAuthor { get; set; }

        public string FullAuthor { get; set; }

        public List<string> LeadAuthorList { get; set; }

        public List<string> FeatureAuthorList { get; set; }

        public int Score { get; set; }

        public byte[] CoverArt { get; set; }

        public string Cover { get; set; }

        public Song(string title)
        {
            LeadAuthorList = new List<string>();
            FeatureAuthorList = new List<string>();
            Title = title;
            Score = 0;
            LeadAuthor = string.Empty;
            FeatureAuthor = string.Empty;
            FullAuthor = string.Empty;
        }
    }
}
