using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IOrderOperations : IService
    {
        [OperationContract]
        void AddArticle();
        [OperationContract]
        void RemoveArticle();
        [OperationContract]
        void CheckOut();
    }
}
