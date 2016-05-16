using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.TVShows;

namespace Trakttv.TraktAPI.Model
{
    [DataContract]
    public class TraktEpisodeSummaryEx
    {
        [DataMember(Name = "episode")]
        public TVEpisodeSummary Episode { get; set; }

        [DataMember(Name = "show")]
        public TVShowSummary Show { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}x{2} - {3}", this.Show.Title, Episode.Season, Episode.Number, Episode.Title ?? "TBA");
        }
    }
}
