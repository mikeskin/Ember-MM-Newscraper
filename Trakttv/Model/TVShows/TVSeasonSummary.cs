using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TVSeasonSummary : TVSeason
    {
        [DataMember(Name = "episode_count")]
        public int EpisodeCount { get; set; }

        [DataMember(Name = "images")]
        public TVSeasonImages Images { get; set; }

        [DataMember(Name = "overview")]
        public string Overview { get; set; }

        [DataMember(Name = "rating")]
        public double? Rating { get; set; }

        [DataMember(Name = "votes")]
        public int Votes { get; set; }
    }
}
