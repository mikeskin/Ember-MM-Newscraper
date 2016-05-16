using System.Collections.Generic;
using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.Movies;
using Trakttv.TraktAPI.Model.TVShows;

namespace Trakttv.TraktAPI.Model
{
    /// <summary>
    /// Data structure for a response from Trakt
    /// </summary>
    [DataContract]
    public class TraktResponse
    {
        [DataMember(Name = "added")]
        public Items Added { get; set; }

        [DataMember(Name = "deleted")]
        public Items Deleted { get; set; }

        [DataMember(Name = "existing")]
        public Items Existing { get; set; }

        [DataContract]
        public class Items
        {
            [DataMember(Name = "movies")]
            public int Movies { get; set; }

            [DataMember(Name = "shows")]
            public int Shows { get; set; }

            [DataMember(Name = "seasons")]
            public int Seasons { get; set; }

            [DataMember(Name = "episodes")]
            public int Episodes { get; set; }

            [DataMember(Name = "people")]
            public int People { get; set; }
        }

        [DataMember(Name = "not_found")]
        public NotFoundObjects NotFound { get; set; }

        [DataContract]
        public class NotFoundObjects
        {
            [DataMember(Name = "movies")]
            public List<Movie> Movies { get; set; }

            [DataMember(Name = "shows")]
            public List<TVShow> Shows { get; set; }

            [DataMember(Name = "episodes")]
            public List<TVEpisode> Episodes { get; set; }

            [DataMember(Name = "seasons")]
            public List<TVSeason> Seasons { get; set; }

            [DataMember(Name = "people")]
            public List<TraktPerson> People { get; set; }
        }
    }
}
