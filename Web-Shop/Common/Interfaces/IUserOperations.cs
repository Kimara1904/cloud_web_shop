using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IUserOperations : IService
    {
        [OperationContract]
        bool Login();
        [OperationContract]
        void Register();
        [OperationContract]
        void ModifyProfile();
    }
}
