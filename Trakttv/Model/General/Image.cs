using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.General
{
    [DataContract]
    public class Image
    {
        [DataMember(Name = "full")]
        public string FullSize { get; set; }

        [DataMember(Name = "medium")]
        public string MediumSize { get; set; }

        [DataMember(Name = "thumb")]
        public string ThumbSize { get; set; }
    }
}
