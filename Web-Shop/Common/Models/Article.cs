using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class Article
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public string Name { get; set; } = null!;
        [DataMember]
        public string Description { get; set; } = null!;
        [DataMember]
        public string Category { get; set; } = null!;
        [DataMember]
        public double Price { get; set; }
        [DataMember]
        public int Amount { get; set; }

    }
}
