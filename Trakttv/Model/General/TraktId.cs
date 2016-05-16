using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.General
{
    [DataContract]
    public class TraktId
    {
        [DataMember(Name = "slug")]
        public string Slug { get; set; }

        [DataMember(Name = "trakt")]
        public int? Trakt { get; set; }
    }
}
