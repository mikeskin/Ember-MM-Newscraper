using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.Movies
{
    [DataContract]
    public class TraktSyncMovieWatched : Movie
    {
        [DataMember(Name = "watched_at")]
        public string WatchedAt { get; set; }
    }
}
