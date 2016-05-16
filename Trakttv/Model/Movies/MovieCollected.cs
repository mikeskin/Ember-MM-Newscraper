using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.Movies
{
    [DataContract]
    public class MovieCollected
    {
        [DataMember(Name = "collected_at")]
        public string CollectedAt { get; set; }

        [DataMember(Name = "movie")]
        public Movie Movie { get; set; }
    }
}
