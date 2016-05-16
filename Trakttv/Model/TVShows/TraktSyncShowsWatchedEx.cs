using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TraktSyncShowsWatchedEx
    {
        [DataMember(Name = "shows")]
        public List<TraktSyncShowWatchedEx> Shows { get; set; }
    }
}
