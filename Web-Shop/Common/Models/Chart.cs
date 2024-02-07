using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class Chart
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public List<ChartItem> Items { get; set; } = null!;
        [DataMember]
        public string Address { get; set; } = null!;
        [DataMember]
        public double TotalPrice { get; set; } = 0.0;
        [DataMember]
        public long BuyerId { get; set; }
    }
}
