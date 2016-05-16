using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.Histories
{
    [DataContract]
    public class HistoryBase
    {
        [DataMember(Name = "action")]
        public string Action { get; set; }

        [DataMember(Name = "id")]
        public int HistoryId { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "watched_at")]
        public string WatchedAt { get; set; }
    }
}