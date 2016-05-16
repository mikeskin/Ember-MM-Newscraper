using System.Collections.Generic;
using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.TVShows;

namespace Trakttv.TraktAPI.Model.Histories
{
    [DataContract]
    public class WatchedTVShowsResponse : WatchedBase
    {
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
            public class Episode : WatchedBase
            {
                [DataMember(Name = "number")]
                public int Number { get; set; }
            }
        }
    }
}