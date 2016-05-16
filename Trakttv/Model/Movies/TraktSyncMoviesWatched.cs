using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.Movies
{
    [DataContract]
    public class TraktSyncMoviesWatched
    {
        [DataMember(Name = "movies")]
        public List<TraktSyncMovieWatched> Movies { get; set; }
    }
}
