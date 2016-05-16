using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TVEpisodeCollected
    {
        [DataMember(Name = "last_collected_at")]
        public string CollectedAt { get; set; }

        [DataMember(Name = "show")]
        public TVShow Show { get; set; }
        
        [DataMember(Name = "seasons")]
        public List<Season> Seasons { get; set; }

        [DataContract]
        public class Season
        {
            [DataMember(Name = "number")]
            public int Number { get; set; }

            [DataMember(Name = "episodes")]
            public List<Episode> Episodes { get; set; }

            [DataContract]
            public class Episode
            {
                [DataMember(Name = "number")]
                public int Number { get; set; }

                [DataMember(Name = "collected_at")]
                public string CollectedAt { get; set; }
            }
        }
    }
}
