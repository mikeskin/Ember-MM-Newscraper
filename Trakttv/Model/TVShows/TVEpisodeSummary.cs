using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TVEpisodeSummary : TVEpisode
    {
        [DataMember(Name = "available_translations")]
        public List<string> AvailableTranslations { get; set; }

        [DataMember(Name = "first_aired")]
        public string FirstAired { get; set; }

        [DataMember(Name = "images")]
        public TVEpisodeImages Images { get; set; }

        [DataMember(Name = "number_abs")]
        public int? AbsoluteNumber { get; set; }

        [DataMember(Name = "overview")]
        public string Overview { get; set; }

        [DataMember(Name = "rating")]
        public double? Rating { get; set; }

        [DataMember(Name = "updated_at")]
        public string UpdatedAt { get; set; }

        [DataMember(Name = "votes")]
        public int Votes { get; set; }
    }
}
