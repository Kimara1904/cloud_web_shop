using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IArticleOperations : IService
    {
        [OperationContract]
        bool Check();
        [OperationContract]
        void PutInChart();
        [OperationContract]
        void RemoveFromChart();
    }
}
