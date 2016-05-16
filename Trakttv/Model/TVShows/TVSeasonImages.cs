using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.General;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TVSeasonImages
    {
        [DataMember(Name = "poster")]
        public Image Poster { get; set; }
        
        [DataMember(Name = "thumb")]
        public Image Thumb { get; set; }
    }
}
