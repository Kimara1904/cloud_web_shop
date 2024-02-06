using Common.DTOs;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IUserService : IService
    {
        [OperationContract]
        Task<UserDTO?> GetUser(string username, string password);
        [OperationContract]
        Task<string> CreateUser(RegisterDTO userDTO);
        [OperationContract]
        Task<Tuple<string, UserDTO?>> ModifyUser(ModifyDTO userDTO);
    }
}
