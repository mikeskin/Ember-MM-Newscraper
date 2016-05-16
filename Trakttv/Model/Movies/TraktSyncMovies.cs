using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.Movies
{
    [DataContract]
    public class TraktSyncMovies
    {
        [DataMember(Name = "movies")]
        public List<Movie> Movies { get; set; }
    }
}
