using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TVEpisode
    {
        [DataMember(Name = "ids")]
        public TVEpisodeIds Ids { get; set; }

        [DataMember(Name = "number")]
        public int Number { get; set; }

        [DataMember(Name = "season")]
        public int Season { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }
    }
}
