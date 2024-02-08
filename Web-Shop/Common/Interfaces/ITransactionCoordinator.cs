using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface ITransactionCoordinator : IService
    {
        [OperationContract]
        Task<Tuple<bool, string>> Prepare(List<ChartItem> articles);
        [OperationContract]
        Task<string> Commit(Chart newChart);
        [OperationContract]
        Task RollBack();
    }
}
