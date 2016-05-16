using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TraktSyncEpisodeWatched : TVEpisode
    {
        [DataMember(Name = "watched_at")]
        public string WatchedAt { get; set; }
    }
}
