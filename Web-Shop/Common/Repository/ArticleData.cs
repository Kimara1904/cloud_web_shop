using Azure;
using Azure.Data.Tables;
using Common.Models;

namespace Common.Repository
{
    public class ArticleData : ITableEntity
    {
        public ArticleData(Article article)
        {
            if (article.Id == 0)
            {
                long id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                this.Id = id;
                RowKey = id.ToString();
            }
            else
            {
                this.Id = article.Id;
                RowKey = article.Id.ToString();
            }
            this.Name = article.Name;
            this.Description = article.Description;
            this.Category = article.Category;
            this.Price = article.Price;
            this.Amount = article.Amount;
            PartitionKey = "Article";
        }

        public ArticleData() { }

        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Category { get; set; } = null!;
        public double Price { get; set; }
        public int Amount { get; set; }
        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
