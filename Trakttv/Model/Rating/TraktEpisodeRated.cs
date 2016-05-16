using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.TVShows;

namespace Trakttv.TraktAPI.Model
{
    [DataContract]
    public class TraktEpisodeRated
    {
        [DataMember(Name = "rating")]
        public int Rating { get; set; }

        [DataMember(Name = "rated_at")]
        public string RatedAt { get; set; }

        [DataMember(Name = "episode")]
        public TVEpisode Episode { get; set; }

        [DataMember(Name = "show")]
        public TVShow Show { get; set; }
    }
}