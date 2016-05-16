using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.General;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TVShowImages
    {
        [DataMember(Name = "banner")]
        public Image Banner { get; set; }

        [DataMember(Name = "clearart")]
        public Image ClearArt { get; set; }
        
        [DataMember(Name = "fanart")]
        public Image Fanart { get; set; }

        [DataMember(Name = "logo")]
        public Image Logo { get; set; }

        [DataMember(Name = "poster")]
        public Image Poster { get; set; }

        [DataMember(Name = "thumb")]
        public Image Thumb { get; set; }
    }
}
