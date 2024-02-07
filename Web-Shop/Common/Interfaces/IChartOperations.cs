using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IChartOperations : IService
    {
        [OperationContract]
        Task<List<Chart>> GetCharts(long buyerId);
        [OperationContract]
        Task<string> CheckOut(Chart newChart);
        [OperationContract]
        Task GetPerviousState();
    }
}
