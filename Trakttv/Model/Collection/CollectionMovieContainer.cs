using System.Runtime.Serialization;
using Trakttv.TraktAPI.Model.General;
using Trakttv.TraktAPI.Model.Movies;

namespace Trakttv.TraktAPI.Model.Collection
{
    [DataContract]
    public class CollectionMovieContainer : Movie
    {
        [DataMember(Name = "collected_at")]
        public string CollectedAt { get; set; }

        public Metadata Metadata { get; set; }

        [DataMember(Name = "media_type")]
        private string MediaType
        {
            get
            {
                return Metadata.MediaType;
            }
            set
            {
                Metadata.MediaType = value;
            }
        }

        [DataMember(Name = "resolution")]
        private string Resolution
        {
            get
            {
                return Metadata.Resolution;
            }
            set
            {
                Metadata.Resolution = value;
            }
        }

        [DataMember(Name = "audio")]
        private string Audio
        {
            get
            {
                return Metadata.Audio;
            }
            set
            {
                Metadata.Audio = value;
            }
        }

        [DataMember(Name = "audio_channels")]
        private string Audiochannels
        {
            get
            {
                return Metadata.AudioChannels;
            }
            set
            {
                Metadata.AudioChannels = value;
            }
        }

        [DataMember(Name = "3d")]
        private bool Stereo3D
        {
            get
            {
                return Metadata.Stereo3D;
            }
            set
            {
                Metadata.Stereo3D = value;
            }
        }
    }
}
