using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Common.Interfaces;
using Common.Models;
using Common.Repository;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace ArticleService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ArticleService : StatefulService, IArticleOperations
    {
        public ArticleService(StatefulServiceContext context)
            : base(context)
        { }

        private async Task Initialize()
        {
            var articles = new List<Article>
            {
                new Article
                {
                    Id = 1,
                    Name = "Milk",
                    Description = "Fresh cow milk",
                    Category = "Dairy",
                    Price = 1.99,
                    Amount = 50
                },
                new Article
                {
                    Id = 2,
                    Name = "Eggs",
                    Description = "Farm-fresh eggs",
                    Category = "Dairy",
                    Price = 2.49,
                    Amount = 40
                },
                new Article
                {
                    Id = 3,
                    Name = "Cheese",
                    Description = "Imported Swiss cheese",
                    Category = "Dairy",
                    Price = 5.99,
                    Amount = 20
                },
                new Article
                {
                    Id = 4,
                    Name = "Apples",
                    Description = "Fresh apples from the orchard",
                    Category = "Fruits",
                    Price = 0.99,
                    Amount = 100
                },
                new Article
                {
                    Id = 5,
                    Name = "Bananas",
                    Description = "Ripe bananas",
                    Category = "Fruits",
                    Price = 1.49,
                    Amount = 80
                },
                new Article
                {
                    Id = 6,
                    Name = "Oranges",
                    Description = "Juicy oranges",
                    Category = "Fruits",
                    Price = 1.29,
                    Amount = 70
                },
                new Article
                {
                    Id = 7,
                    Name = "Bread",
                    Description = "Freshly baked bread",
                    Category = "Bakery",
                    Price = 2.49,
                    Amount = 30
                },
                new Article
                {
                    Id = 8,
                    Name = "Croissants",
                    Description = "Buttery croissants",
                    Category = "Bakery",
                    Price = 1.99,
                    Amount = 40
                },
                new Article
                {
                    Id = 9,
                    Name = "Bagels",
                    Description = "Toasted bagels",
                    Category = "Bakery",
                    Price = 3.49,
                    Amount = 20
                }
            };

            string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

            TableServiceClient tableServiceClient = new(connectionString);
            TableItem tableItem = await tableServiceClient.CreateTableIfNotExistsAsync("Article");
            TableClient articleClient = tableServiceClient.GetTableClient("Article");

            var operationData = new DataOperations<ArticleData>(articleClient);

            var stateManager = this.StateManager;
            var articleDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Article>>("articleDictionary");

            var articleList = new List<ArticleData>();

            using var transaction = stateManager.CreateTransaction();
            await foreach (var item in operationData.RetrieveAllAsync("Article"))
            {
                articleList.Add(item);
                var articleVal = new Article
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Category = item.Category,
                    Amount = item.Amount,
                    Price = item.Price
                };
                await articleDictionary.AddOrUpdateAsync(transaction, item.Id, articleVal, (k, v) => articleVal);
            }

            if (articleList.Count == 0)
            {
                foreach (var item in articles)
                {
                    await operationData.AddAsync(new ArticleData(item));

                    var articleVal = new Article
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        Category = item.Category,
                        Amount = item.Amount,
                        Price = item.Price
                    };
                    await articleDictionary.AddOrUpdateAsync(transaction, item.Id, articleVal, (k, v) => articleVal);
                }
            }
            await transaction.CommitAsync();
        }

        private async Task UpdateState()
        {
            string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

            TableServiceClient tableServiceClient = new(connectionString);

            TableItem tableItem = await tableServiceClient.CreateTableIfNotExistsAsync("Article");
            TableClient articleClient = tableServiceClient.GetTableClient("Article");
            var operationData = new DataOperations<ArticleData>(articleClient);

            var stateManager = this.StateManager;
            var articleDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Article>>("articleDictionary");

            using var transaction = stateManager.CreateTransaction();
            var articleEnumerator = (await articleDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();

            while (await articleEnumerator.MoveNextAsync(CancellationToken.None))
            {
                var article = articleEnumerator.Current;
                await operationData.Modify(new ArticleData(article.Value));
            }
        }

        public async Task<List<Article>> GetArticles(string category)
        {
            var stateManager = this.StateManager;
            var articleDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Article>>("articleDictionary");

            using var transaction = stateManager.CreateTransaction();
            var articleEnumerator = (await articleDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();

            var articles = new List<Article>();

            while (await articleEnumerator.MoveNextAsync(CancellationToken.None))
            {
                var article = articleEnumerator.Current;
                if (category.Equals("All") || article.Value.Category.Equals(category))
                {
                    articles.Add(article.Value);
                }
            }

            return articles;
        }


        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            await Initialize();

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await UpdateState();

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        public async Task<Tuple<bool, string>> CheckArticles(List<ChartItem> articles)
        {
            var stateManager = this.StateManager;
            var articleDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Article>>("articleDictionary");

            using var transaction = stateManager.CreateTransaction();
            var articleEnumerator = (await articleDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();

            while (await articleEnumerator.MoveNextAsync(CancellationToken.None))
            {
                var article = articleEnumerator.Current;
                var chartItem = articles.FirstOrDefault(x => x.ArticleId == article.Value.Id);
                if (chartItem != null && chartItem.Amount > article.Value.Amount)
                {
                    return new Tuple<bool, string>(false, $"There is no enough {article.Value.Name}");
                }
            }

            return new Tuple<bool, string>(true, "Everything is okay");
        }

        public async Task<string> PutInChart(List<ChartItem> articles)
        {
            var stateManager = this.StateManager;
            var articleDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Article>>("articleDictionary");
            var pervArticleDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Article>>("pervArticleDictionary");

            using var transaction = stateManager.CreateTransaction();
            var articleEnumerator = (await articleDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();

            while (await articleEnumerator.MoveNextAsync(CancellationToken.None))
            {
                var article = articleEnumerator.Current;
                await pervArticleDictionary.AddAsync(transaction, article.Key, article.Value);
                var chartItem = articles.FirstOrDefault(x => x.ArticleId == article.Value.Id);
                if (chartItem != null)
                {
                    article.Value.Amount -= chartItem.Amount;
                }
            }

            return "Successfully filled chart";
        }

        public async Task GetPerviousState()
        {
            var stateManager = this.StateManager;
            var articleDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Article>>("articleDictionary");
            var prevArticleDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Article>>("prevArticleDictionary");

            using var transaction = stateManager.CreateTransaction();

            if (await prevArticleDictionary.GetCountAsync(transaction) == 0)
            {
                return;
            }

            var enumerablePrev = await prevArticleDictionary.CreateEnumerableAsync(transaction);
            var enumerator = enumerablePrev.GetAsyncEnumerator();

            while (await enumerator.MoveNextAsync(CancellationToken.None))
            {
                var current = enumerator.Current;
                await articleDictionary.AddOrUpdateAsync(transaction, current.Key, current.Value, (k, v) => current.Value);
            }

            await prevArticleDictionary.ClearAsync();

            await transaction.CommitAsync();
        }
    }
}
