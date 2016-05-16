using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.Histories
{
    [DataContract]
    public class WatchedBase
    {
        [DataMember(Name = "last_watched_at")]
        public string LastWatchedAt { get; set; }

        [DataMember(Name = "plays")]
        public int Plays { get; set; }
    }
}