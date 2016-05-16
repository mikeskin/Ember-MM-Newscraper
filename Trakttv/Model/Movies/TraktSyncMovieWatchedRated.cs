using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.Movies
{
    [DataContract]
    public class TraktMovieWatchedRated
    {
        [DataMember(Name = "plays")]
        public int Plays { get; set; }

        [DataMember(Name = "last_watched_at")]
        public string LastWatchedAt { get; set; }

        [DataMember(Name = "movie")]
        public Movie Movie { get; set; }

        [DataMember(Name = "rating")]
        public int Rating { get; set; }

        [DataMember(Name = "rated_at")]
        public string RatedAt { get; set; }
        public bool Modified { get; set; }

    }
}
