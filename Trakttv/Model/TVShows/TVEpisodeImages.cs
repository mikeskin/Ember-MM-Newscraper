using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.General;

namespace Trakttv.TraktAPI.Model.TVShows
{
    [DataContract]
    public class TVEpisodeImages
    {
        [DataMember(Name = "screenshot")]
        public Image ScreenShot { get; set; }
    }
}
