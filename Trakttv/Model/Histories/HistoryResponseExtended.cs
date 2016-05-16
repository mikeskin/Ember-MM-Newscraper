using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.Movies;
using Trakttv.TraktAPI.Model.TVShows;

namespace Trakttv.TraktAPI.Model.Histories
{
    [DataContract]
    public class HistoryResponseExtended : HistoryBase
    {
        [DataMember(Name = "episode")]
        public TVEpisodeSummary Episode { get; set; }

        [DataMember(Name = "movie")]
        public MovieSummary Movie { get; set; }

        [DataMember(Name = "show")]
        public TVShowSummary Show { get; set; }
    }
}