using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.Movies;
using Trakttv.TraktAPI.Model.TVShows;

namespace Trakttv.TraktAPI.Model.Histories
{
    [DataContract]
    public class HistoryResponse : HistoryBase
    {
        [DataMember(Name = "episode")]
        public TVEpisode Episode { get; set; }

        [DataMember(Name = "movie")]
        public Movie Movie { get; set; }

        [DataMember(Name = "show")]
        public TVShow Show { get; set; }
    }
}