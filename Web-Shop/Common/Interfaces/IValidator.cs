using Common.DTOs;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IValidator : IService
    {
        [OperationContract]
        Task<string> LoginValidator(string username, string password);
        [OperationContract]
        Task<string> RegisterValidator(UserDTO newUser);
        [OperationContract]
        Task<string> ModifyValidator(UserDTO user);
        [OperationContract]
        Task<List<Article>> ArticleViewValidator();
        [OperationContract]
        void ChartViewValidator();
        [OperationContract]
        Task<string> AddInChartValidator(int id, int amount);
        [OperationContract]
        Task<string> RemoveFromChartValidator(int id);
        [OperationContract]
        Task<string> CheckoutValidator(int id);
    }
}
