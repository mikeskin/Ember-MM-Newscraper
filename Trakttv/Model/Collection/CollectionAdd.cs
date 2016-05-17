using System.Collections.Generic;
using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.General;
using Trakttv.TraktAPI.Model.Movies;

namespace Trakttv.TraktAPI.Model.Collection
{
    [DataContract]
    public class CollectionAdd
    {
        [DataMember(Name = "movies")]
        public List<CollectionMovieContainer> Movies { get; set; }
    }
}
