using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TraktSeasonRated
    {
        [DataMember(Name = "rating")]
        public int Rating { get; set; }

        [DataMember(Name = "rated_at")]
        public string RatedAt { get; set; }

        [DataMember(Name = "show")]
        public TVShow Show { get; set; }

        [DataMember(Name = "season")]
        public TVSeason Season { get; set; }
    }
}