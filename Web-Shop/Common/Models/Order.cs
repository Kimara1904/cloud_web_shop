using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class Order
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public virtual List<OrderItem> Items { get; set; } = null!;
        [DataMember]
        public string Address { get; set; } = null!;
        [DataMember]
        public DateTime DeliveryTime { get; set; }
        [DataMember]
        public string? Comment { get; set; }
        [DataMember]
        public double DeliveryPrice { get; set; }
        [DataMember]
        public virtual User? Buyer { get; set; }
        [DataMember]
        public int BuyerId { get; set; }
    }
}
