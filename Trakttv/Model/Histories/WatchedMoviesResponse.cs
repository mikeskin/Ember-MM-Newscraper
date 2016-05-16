using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.Movies;

namespace Trakttv.TraktAPI.Model.Histories
{
    [DataContract]
    public class WatchedMoviesResponse : WatchedBase
    {
        [DataMember(Name = "movie")]
        public Movie Movie { get; set; }
    }
}
