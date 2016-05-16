using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.Movies;

namespace Trakttv.TraktAPI.Model.Histories
{
    [DataContract]
    public class WatchedMoviesResponseExtended : WatchedBase
    {
        [DataMember(Name = "movie")]
        public MovieSummary Movie { get; set; }
    }
}
