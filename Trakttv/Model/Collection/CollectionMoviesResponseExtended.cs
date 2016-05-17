using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.General;
using Trakttv.TraktAPI.Model.Movies;

namespace Trakttv.TraktAPI.Model.Collection
{
    [DataContract]
    public class CollectionMoviesResponseExtended : Metadata
    {
        [DataMember(Name = "collected_at")]
        public string CollectedAt { get; set; }

        [DataMember(Name = "movie")]
        public Movie Movie { get; set; }
    }
}
