using System.Runtime.Serialization;

namespace Common.Messages
{
    [DataContract]
    public class PubMessage
    {
        [DataMember]
        public string Content { get; set; } = null!;
    }
}
