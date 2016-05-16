using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.Movies;
using Trakttv.TraktAPI.Model.TVShows;

namespace Trakttv.TraktAPI.Model
{
    [DataContract]
    public class TraktCommentItem
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "movie")]
        public MovieSummary Movie { get; set; }

        [DataMember(Name = "show")]
        public TVShowSummary Show { get; set; }

        [DataMember(Name = "season")]
        public TVSeasonSummary Season { get; set; }

        [DataMember(Name = "episode")]
        public TVEpisodeSummary Episode { get; set; }

        [DataMember(Name = "list")]
        public TraktListDetail List { get; set; }

        [DataMember(Name = "comment")]
        public TraktComment Comment { get; set; }
    }
}
