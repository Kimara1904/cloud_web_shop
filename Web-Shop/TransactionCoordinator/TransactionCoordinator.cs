using Common.Interfaces;
using Common.Models;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace TransactionCoordinator
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TransactionCoordinator : StatefulService, ITransactionCoordinator
    {
        public TransactionCoordinator(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<string> Commit(Chart newChart)
        {
            var fabricClient = new FabricClient();
            var articleServiceUri = new Uri("fabric:/Web-Shop/ArticleService");
            var articlePartitionList = await fabricClient.QueryManager.GetPartitionListAsync(articleServiceUri);

            IArticleOperations articleProxy = null!;

            foreach (var chartPartition in articlePartitionList)
            {
                var partitionKey = chartPartition.PartitionInformation as Int64RangePartitionInformation;

                if (partitionKey != null)
                {
                    var servicePartitionKey = new ServicePartitionKey(partitionKey.LowKey);

                    articleProxy = ServiceProxy.Create<IArticleOperations>(articleServiceUri, servicePartitionKey);
                    break;
                }
            }

            var chartServiceUri = new Uri("fabric:/Web-Shop/ChartService");
            var chartPartitionList = await fabricClient.QueryManager.GetPartitionListAsync(chartServiceUri);

            IChartOperations chartProxy = null!;

            foreach (var chartPartition in chartPartitionList)
            {
                var partitionKey = chartPartition.PartitionInformation as Int64RangePartitionInformation;

                if (partitionKey != null)
                {
                    var servicePartitionKey = new ServicePartitionKey(partitionKey.LowKey);

                    chartProxy = ServiceProxy.Create<IChartOperations>(chartServiceUri, servicePartitionKey);
                    break;
                }
            }

            try
            {
                await articleProxy.PutInChart(newChart.Items);
                return await chartProxy.CheckOut(newChart);
            }
            catch (Exception ex)
            {
                return $"Error in communication with service {ex.Message}";
            }
        }

        public async Task<Tuple<bool, string>> Prepare(List<ChartItem> articles)
        {
            var fabricClient = new FabricClient();
            var articleServiceUri = new Uri("fabric:/Web-Shop/ArticleService");
            var articlePartitionList = await fabricClient.QueryManager.GetPartitionListAsync(articleServiceUri);

            IArticleOperations proxy = null!;

            foreach (var chartPartition in articlePartitionList)
            {
                var partitionKey = chartPartition.PartitionInformation as Int64RangePartitionInformation;

                if (partitionKey != null)
                {
                    var servicePartitionKey = new ServicePartitionKey(partitionKey.LowKey);

                    proxy = ServiceProxy.Create<IArticleOperations>(articleServiceUri, servicePartitionKey);
                    break;
                }
            }

            try
            {
                return await proxy.CheckArticles(articles);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, $"Error in communication with service {ex.Message}");
            }
        }

        public async Task RollBack()
        {
            var fabricClient = new FabricClient();
            var chartServiceUri = new Uri("fabric:/Web-Shop/ChartService");
            var chartPartitionList = await fabricClient.QueryManager.GetPartitionListAsync(chartServiceUri);

            IChartOperations chartProxy = null!;

            foreach (var chartPartition in chartPartitionList)
            {
                var partitionKey = chartPartition.PartitionInformation as Int64RangePartitionInformation;

                if (partitionKey != null)
                {
                    var servicePartitionKey = new ServicePartitionKey(partitionKey.LowKey);

                    chartProxy = ServiceProxy.Create<IChartOperations>(chartServiceUri, servicePartitionKey);
                    break;
                }
            }

            var articleServiceUri = new Uri("fabric:/Web-Shop/ArticleService");
            var articlePartitionList = await fabricClient.QueryManager.GetPartitionListAsync(articleServiceUri);

            IArticleOperations articleProxy = null!;

            foreach (var chartPartition in articlePartitionList)
            {
                var partitionKey = chartPartition.PartitionInformation as Int64RangePartitionInformation;

                if (partitionKey != null)
                {
                    var servicePartitionKey = new ServicePartitionKey(partitionKey.LowKey);

                    articleProxy = ServiceProxy.Create<IArticleOperations>(articleServiceUri, servicePartitionKey);
                    break;
                }
            }

            await chartProxy.GetPerviousState();
            await articleProxy.GetPerviousState();
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

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
