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
        Task<Tuple<string, UserDTO?>> LoginValidator(string username, string password);
        [OperationContract]
        Task<string> RegisterValidator(RegisterDTO newUser);
        [OperationContract]
        Task<Tuple<string, UserDTO?>> ModifyValidator(ModifyDTO user);
        [OperationContract]
        Task<Tuple<string, List<Article>?>> ArticleViewValidator(UserDTO user, string category);
        [OperationContract]
        Task ChartViewValidator();
        [OperationContract]
        Task<string> AddInChartValidator(int id, int amount);
        [OperationContract]
        Task<string> RemoveFromChartValidator(int id);
        [OperationContract]
        Task<string> CheckoutValidator(int id);
    }
}
