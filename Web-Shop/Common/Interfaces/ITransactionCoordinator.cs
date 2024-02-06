using Common.DTOs;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interfaces
{
    public interface ITransactionCoordinator : IService
    {
        Task<UserDTO> Login(string username, string password);
        Task<string> RegisterCommit(UserDTO newUser);
    }
}
