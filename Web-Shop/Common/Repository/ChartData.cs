using Azure;
using Azure.Data.Tables;
using Common.Models;

namespace Common.Repository
{
    public class ChartData : ITableEntity
    {
        public ChartData(Chart chart)
        {
            if (chart.Id == 0)
            {
                long id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                this.Id = id;
                RowKey = id.ToString();
            }
            else
            {
                this.Id = chart.Id;
                RowKey = chart.Id.ToString();
            }
            this.Address = chart.Address;
            this.TotalPrice = chart.TotalPrice;
            this.BuyerId = chart.BuyerId;
            PartitionKey = "Chart";
        }
        public ChartData()
        {

        }

        public long Id { get; set; }
        public string Address { get; set; } = null!;
        public double TotalPrice { get; set; }
        public long BuyerId { get; set; }
        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
