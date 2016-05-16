using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TraktSyncShows
    {
        [DataMember(Name = "shows")]
        public List<TVShow> Shows { get; set; }
    }
}
