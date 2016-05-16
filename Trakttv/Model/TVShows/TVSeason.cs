using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TVSeason
    {
        [DataMember(Name = "ids")]
        public TVSeasonIds Ids { get; set; }

        [DataMember(Name = "number")]
        public int Number { get; set; }
    }
}
