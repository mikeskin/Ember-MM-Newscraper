using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.General;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TVShowIds : TraktId
    {
        [DataMember(Name = "imdb")]
        public string Imdb { get; set; }

        [DataMember(Name = "tmdb")]
        public int? Tmdb { get; set; }

        [DataMember(Name = "tvdb")]
        public int? Tvdb { get; set; }

        [DataMember(Name = "tvrage")]
        public int? TvRage { get; set; }
    }
}
