using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TraktSyncShowsEx
    {
        [DataMember(Name = "shows")]
        public List<TraktSyncShowEx> Shows { get; set; }
    }
}
