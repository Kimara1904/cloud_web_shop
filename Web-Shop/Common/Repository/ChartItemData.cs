using Azure;
using Azure.Data.Tables;
using Common.Models;

namespace Common.Repository
{
    public class ChartItemData : ITableEntity
    {
        public ChartItemData(ChartItem chartItem)
        {
            if (chartItem.Id == 0)
            {
                long id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                this.Id = id;
                RowKey = id.ToString();
            }
            else
            {
                this.Id = chartItem.Id;
                RowKey = chartItem.Id.ToString();
            }
            this.ArticleId = chartItem.ArticleId;
            this.ArticlePrice = chartItem.ArticlePrice;
            this.ArticleName = chartItem.ArticleName;
            this.Amount = chartItem.Amount;
            PartitionKey = "ChartItem";
        }

        public ChartItemData() { }

        public long Id { get; set; }
        public long ArticleId { get; set; }
        public string ArticleName { get; set; } = null!;
        public double ArticlePrice { get; set; }
        public int Amount { get; set; }
        public long ChartId { get; set; }
        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
