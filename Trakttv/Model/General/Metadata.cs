using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.General
{
    [DataContract]
    public class Metadata
    {
        [DataMember(Name = "3d")]
        public bool Stereo3D { get; set; }

        [DataMember(Name = "audio")]
        public string Audio { get; set; }

        [DataMember(Name = "audio_channels")]
        public string AudioChannels { get; set; }

        [DataMember(Name = "media_type")]
        public string MediaType { get; set; }

        [DataMember(Name = "resolution")]
        public string Resolution { get; set; }
    }
}
