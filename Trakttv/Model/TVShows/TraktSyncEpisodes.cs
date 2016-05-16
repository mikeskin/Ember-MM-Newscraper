using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TraktSyncEpisodes
    {
        [DataMember(Name = "episodes")]
        public List<TVEpisode> Episodes { get; set; }
    }
}
