using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.Movies;

namespace Trakttv.TraktAPI.Model
{
    [DataContract]
    public class TraktSyncMovieRated : Movie
    {
        [DataMember(Name = "rated_at")]
        public string RatedAt { get; set; }

        [DataMember(Name = "rating")]
        public int Rating { get; set; }
    }
}
