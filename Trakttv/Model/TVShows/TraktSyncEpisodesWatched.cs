using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TraktSyncEpisodesWatched
    {
        [DataMember(Name = "episodes")]
        public List<TraktSyncEpisodeWatched> Episodes { get; set; }
    }
}
