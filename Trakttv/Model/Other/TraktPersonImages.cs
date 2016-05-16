using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.General;

namespace Trakttv.TraktAPI.Model
{
    [DataContract]
    public class TraktPersonImages
    {
        [DataMember(Name = "headshot")]
        public Image HeadShot { get; set; }
    }
}
