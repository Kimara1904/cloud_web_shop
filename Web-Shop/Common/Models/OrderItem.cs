using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class OrderItem
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int ArticleId { get; set; }
        [DataMember]
        public string ArticleName { get; set; } = null!;
        [DataMember]
        public double ArticlePrice { get; set; }
        [DataMember]
        public int Amount { get; set; }
        [DataMember]
        public int OrderId { get; set; }
        [DataMember]
        public virtual Order Order { get; set; } = null!;
    }
}
