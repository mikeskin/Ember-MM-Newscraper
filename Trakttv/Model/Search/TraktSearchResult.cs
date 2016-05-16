using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.Movies;
using Trakttv.TraktAPI.Model.TVShows;

namespace Trakttv.TraktAPI.Model
{
    [DataContract]
    public class TraktSearchResult
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "score")]
        public double? Score { get; set; }

        [DataMember(Name = "movie")]
        public MovieSummary Movie { get; set; }

        [DataMember(Name = "show")]
        public TVShowSummary Show { get; set; }

        [DataMember(Name = "episode")]
        public TVEpisodeSummary Episode { get; set; }

        [DataMember(Name = "person")]
        public TraktPersonSummary Person { get; set; }

        [DataMember(Name = "user")]
        public TraktUserSummary User { get; set; }

        [DataMember(Name = "list")]
        public TraktList List { get; set; }
    }
}