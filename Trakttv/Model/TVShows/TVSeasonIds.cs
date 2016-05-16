using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TVSeasonIds
    {
        [DataMember(Name = "tmdb")]
        public int? Tmdb { get; set; }

        [DataMember(Name = "trakt")]
        public int? Trakt { get; set; }

        [DataMember(Name = "tvdb")]
        public int? Tvdb { get; set; }

        [DataMember(Name = "tvrage")]
        public int? TvRage { get; set; }
    }
}
