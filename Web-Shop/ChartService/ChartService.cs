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

namespace ChartService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ChartService : StatefulService, IChartOperations
    {
        public ChartService(StatefulServiceContext context)
            : base(context)
        { }

        private async Task Initialize()
        {
            string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

            TableServiceClient tableServiceClient = new(connectionString);

            TableItem chartTableItem = await tableServiceClient.CreateTableIfNotExistsAsync("Chart");
            TableClient chartClient = tableServiceClient.GetTableClient("Chart");

            TableItem chartItemTableItem = await tableServiceClient.CreateTableIfNotExistsAsync("ChartItem");
            TableClient chartItemClient = tableServiceClient.GetTableClient("ChartItem");

            var chartOperationData = new DataOperations<ChartData>(chartClient);
            var chartItemOperationData = new DataOperations<ChartItemData>(chartItemClient);

            var stateManager = this.StateManager;
            var userDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Chart>>("chartDictionary");

            using var transaction = stateManager.CreateTransaction();

            var chartItems = chartItemOperationData.RetrieveAllAsync("ChartItem");

            await foreach (var chart in chartOperationData.RetrieveAllAsync("Chart"))
            {
                var items = new List<ChartItem>();
                await foreach (var item in chartItems)
                {
                    if (item.ChartId == chart.Id)
                    {
                        items.Add(new ChartItem
                        {
                            Id = item.Id,
                            ArticleName = item.ArticleName,
                            ArticleId = item.ArticleId,
                            ArticlePrice = item.ArticlePrice,
                            Amount = item.Amount,
                            ChartId = item.ChartId
                        });
                    }
                }

                var chartVal = new Chart
                {
                    Id = chart.Id,
                    Address = chart.Address,
                    Items = items,
                    TotalPrice = chart.TotalPrice,
                    BuyerId = chart.BuyerId
                };
                await userDictionary.AddOrUpdateAsync(transaction, chart.Id, chartVal, (k, v) => chartVal);
            }
            await transaction.CommitAsync();
        }

        private async Task UpdateState()
        {
            string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

            TableServiceClient tableServiceClient = new(connectionString);

            TableItem chartTableItem = await tableServiceClient.CreateTableIfNotExistsAsync("Chart");
            TableClient chartClient = tableServiceClient.GetTableClient("Chart");

            TableItem chartItemTableItem = await tableServiceClient.CreateTableIfNotExistsAsync("ChartItem");
            TableClient chartItemClient = tableServiceClient.GetTableClient("ChartItem");

            var chartOperationData = new DataOperations<ChartData>(chartClient);
            var chartItemOperationData = new DataOperations<ChartItemData>(chartItemClient);

            var stateManager = this.StateManager;
            var chartDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Chart>>("chartDictionary");

            using var transaction = stateManager.CreateTransaction();
            var chartEnumerator = (await chartDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();

            while (await chartEnumerator.MoveNextAsync(CancellationToken.None))
            {
                var chart = chartEnumerator.Current;
                var chartData = new ChartData(chart.Value);
                if (!await chartOperationData.IsThere(chartData.Id.ToString(), "ChartItem"))
                {
                    await chartOperationData.AddAsync(chartData);

                    foreach (var item in chart.Value.Items)
                    {
                        await chartItemOperationData.AddAsync(new ChartItemData(item) { ChartId = chartData.Id });
                    }
                }
            }
        }

        public async Task<string> CheckOut(Chart newChart)
        {
            var stateManager = this.StateManager;
            var chartDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Chart>>("chartDictionary");

            using var transaction = stateManager.CreateTransaction();

            var chartIdHelper = new ChartData(newChart);
            try
            {
                await chartDictionary.AddOrUpdateAsync(transaction, chartIdHelper.Id, newChart, (k, v) => newChart);
                await transaction.CommitAsync();
                return "Successfully created new user";
            }
            catch (Exception ex)
            {
                return $"Error in creation with service {ex.Message}";
            }
        }

        public async Task<List<Chart>> GetCharts(long buyerId)
        {
            var stateManager = this.StateManager;
            var chartDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Chart>>("chartDictionary");

            using var transaction = stateManager.CreateTransaction();
            var chartEnumerator = (await chartDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();

            var charts = new List<Chart>();

            while (await chartEnumerator.MoveNextAsync(CancellationToken.None))
            {
                var chart = chartEnumerator.Current;
                charts.Add(chart.Value);
            }

            return charts;
        }

        public async Task GetPerviousState()
        {
            var stateManager = this.StateManager;
            var chartDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Chart>>("chartDictionary");
            var prevChartDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Chart>>("prevChartDictionary");

            using var transaction = stateManager.CreateTransaction();

            if (await prevChartDictionary.GetCountAsync(transaction) == 0)
            {
                return;
            }

            var enumerablePrev = await prevChartDictionary.CreateEnumerableAsync(transaction);
            var enumerator = enumerablePrev.GetAsyncEnumerator();

            var enumerableNew = await chartDictionary.CreateEnumerableAsync(transaction);
            var NewEnumerator = enumerableNew.GetAsyncEnumerator();

            while (await NewEnumerator.MoveNextAsync(CancellationToken.None))
            {
                var current = NewEnumerator.Current;
                await chartDictionary.TryRemoveAsync(transaction, current.Key);
            }

            while (await enumerator.MoveNextAsync(CancellationToken.None))
            {
                var current = enumerator.Current;
                await chartDictionary.AddAsync(transaction, current.Key, current.Value);
            }

            await prevChartDictionary.ClearAsync();

            await transaction.CommitAsync();
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
    }
}
