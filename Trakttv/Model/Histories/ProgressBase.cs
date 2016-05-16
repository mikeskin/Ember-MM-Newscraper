using System.Runtime.Serialization;

namespace Trakttv.TraktAPI.Model.Histories
{
    [DataContract]
    public class ProgressBase
    {
        [DataMember(Name = "aired")]
        public int Aired { get; set; }

        [DataMember(Name = "completed")]
        public int Completed { get; set; }
    }
}