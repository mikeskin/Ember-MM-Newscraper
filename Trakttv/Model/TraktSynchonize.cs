using System.Collections.Generic;
using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.General;
using Trakttv.TraktAPI.Model.Movies;
using Trakttv.TraktAPI.Model.TVShows;

namespace Trakttv.TraktAPI.Model
{
    [DataContract]
    public class TraktSynchronize
    {
        [DataMember(Name = "movies", EmitDefaultValue = false)]
        public List<Movie> Movies { get; set; }

        [DataMember(Name = "shows", EmitDefaultValue = false)]
        public List<TVShow> Shows { get; set; }

        [DataMember(Name = "seasons", EmitDefaultValue = false)]
        public List<TVSeason> Seasons { get; set; }

        [DataMember(Name = "episodes", EmitDefaultValue = false)]
        public List<TVEpisode> Episodes { get; set; }

        [DataMember(Name = "people", EmitDefaultValue = false)]
        public List<TraktPerson> People { get; set; }

        [DataMember(Name = "ids", EmitDefaultValue = false)]
        public List<TraktId> IDs { get; set; }
    }
}