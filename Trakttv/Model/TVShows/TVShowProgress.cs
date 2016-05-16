using System.Collections.Generic;
using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.Histories;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TVShowProgress : ProgressBase
    {        
        [DataMember(Name = "last_watched_at")]
        public int LastWatchAt { get; set; }
        
        [DataMember(Name = "seasons")]
        public Season Seasons { get; set; }

        [DataContract]
        public class Season : ProgressBase
        {
            [DataMember(Name = "number")]
            public int Number { get; set; }

            [DataMember(Name = "episodes")]
            public List<Episode> Episodes { get; set; }

            [DataContract]
            public class Episode
            {
                [DataMember(Name = "completed")]
                public bool Completed { get; set; }

                [DataMember(Name = "number")]
                public int Number { get; set; }
            }
        }
    }
}
