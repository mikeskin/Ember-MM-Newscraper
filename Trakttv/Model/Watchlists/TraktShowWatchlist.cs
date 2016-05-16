using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.TVShows;

namespace Trakttv.TraktAPI.Model
{
    [DataContract]
    public class TraktShowWatchList
    {
        [DataMember(Name = "listed_at")]
        public string ListedAt { get; set; }

        [DataMember(Name = "show")]
        public TVShowSummary Show { get; set; }
    }
}
