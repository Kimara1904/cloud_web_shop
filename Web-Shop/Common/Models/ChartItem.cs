using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class ChartItem
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public long ArticleId { get; set; }
        [DataMember]
        public string ArticleName { get; set; } = null!;
        [DataMember]
        public double ArticlePrice { get; set; }
        [DataMember]
        public int Amount { get; set; }
        [DataMember]
        public long ChartId { get; set; }
    }
}
