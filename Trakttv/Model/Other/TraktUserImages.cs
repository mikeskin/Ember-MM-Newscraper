using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.General;

namespace Trakttv.TraktAPI.Model
{
    [DataContract]
    public class TraktUserImages
    {
        [DataMember(Name = "avatar")]
        public Image Avatar { get; set; }
    }
}
