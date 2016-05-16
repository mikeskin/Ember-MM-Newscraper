using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.Movies;

namespace Trakttv.TraktAPI.Model
{
    [DataContract]
    public class TraktMovieWatchList
    {
        [DataMember(Name = "listed_at")]
        public string ListedAt { get; set; }

        [DataMember(Name = "movie")]
        public MovieSummary Movie { get; set; }
    }
}
