using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.Movies;

namespace Trakttv.TraktAPI.Model
{
    [DataContract]
    public class TraktCommentMovie
    {

        [DataMember(Name = "movie")]
        public Movie Movie { get; set; }

        [DataMember(Name = "comment")]
        public string Text { get; set; }

        [DataMember(Name = "spoiler")]
        public bool IsSpoiler { get; set; }

    }
}
