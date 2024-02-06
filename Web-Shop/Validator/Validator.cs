using Common.DTOs;
using Common.Interfaces;
using Common.Models;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace Validator
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Validator : StatelessService, IValidator
    {
        public Validator(StatelessServiceContext context)
            : base(context)
        { }

        public async Task<string> AddInChartValidator(int id, int amount)
        {
            if (id <= 0)
            {
                return "Id must be greather than 0";
            }

            if (amount <= 0)
            {
                return "Amount must be greather than 0";
            }

            return "Everything is okay";
        }

        public async Task<Tuple<string, List<Article>?>> ArticleViewValidator(UserDTO user, string category)
        {
            if (user == null)
            {
                return new Tuple<string, List<Article>?>("User must be logged in", null);
            }

            if (string.IsNullOrEmpty(category))
            {
                return new Tuple<string, List<Article>?>("Category is required", null);
            }

            var fabricClient = new FabricClient();
            var serviceUri = new Uri("fabric:/Web-Shop/ArticleService");

            var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(serviceUri);
            IArticleOperations proxy = null!;

            foreach (var partition in partitionList)
            {
                var partitionKey = partition.PartitionInformation as Int64RangePartitionInformation;

                if (partitionKey != null)
                {
                    var servicePartitionKey = new ServicePartitionKey(partitionKey.LowKey);

                    proxy = ServiceProxy.Create<IArticleOperations>(serviceUri, servicePartitionKey);
                    break;
                }
            }

            try
            {
                var articles = await proxy.GetArticles(category);
                return new Tuple<string, List<Article>?>("Success", articles);
            }
            catch (Exception ex)
            {
                return new Tuple<string, List<Article>?>("Error in communication with service " + ex.Message, null);
            }
        }

        public Task ChartViewValidator()
        {
            throw new NotImplementedException();
        }

        public async Task<string> CheckoutValidator(int id)
        {
            if (id <= 0)
            {
                return "Id must be greather than 0";
            }

            return "Everything is okay";
        }

        public async Task<Tuple<string, UserDTO?>> LoginValidator(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new Tuple<string, UserDTO?>("Username is required", null);
            }

            if (string.IsNullOrEmpty(password))
            {
                return new Tuple<string, UserDTO?>("Password is required", null);
            }

            var fabricClient = new FabricClient();
            var serviceUri = new Uri("fabric:/Web-Shop/UserService");

            var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(serviceUri);
            IUserService proxy = null!;

            foreach (var partition in partitionList)
            {
                var partitionKey = partition.PartitionInformation as Int64RangePartitionInformation;

                if (partitionKey != null)
                {
                    var servicePartitionKey = new ServicePartitionKey(partitionKey.LowKey);

                    proxy = ServiceProxy.Create<IUserService>(serviceUri, servicePartitionKey);
                    break;
                }
            }

            try
            {
                var result = await proxy.GetUser(username, password);

                if (result == null)
                {
                    return new Tuple<string, UserDTO?>("Wrong credentials", null);
                }


                return new Tuple<string, UserDTO?>("Successfully login", result);
            }
            catch (Exception ex)
            {
                return new Tuple<string, UserDTO?>("Error in communication with service " + ex.Message, null);
            }
        }

        public async Task<Tuple<string, UserDTO?>> ModifyValidator(ModifyDTO user)
        {
            if (user == null)
            {
                return new Tuple<string, UserDTO?>("User infos are requested", null);
            }

            if (user.Id == 0)
            {
                return new Tuple<string, UserDTO?>("User must be loged in", null);
            }

            if (string.IsNullOrEmpty(user.Username))
            {
                return new Tuple<string, UserDTO?>("Username is required", null);
            }

            if (string.IsNullOrEmpty(user.Password))
            {
                return new Tuple<string, UserDTO?>("Password is requested", null);
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                return new Tuple<string, UserDTO?>("Email is required", null);
            }

            if (string.IsNullOrEmpty(user.FirstName))
            {
                return new Tuple<string, UserDTO?>("First name is required", null);
            }

            if (string.IsNullOrEmpty(user.LastName))
            {
                return new Tuple<string, UserDTO?>("Last name is required", null);
            }

            if (string.IsNullOrEmpty(user.Address))
            {
                return new Tuple<string, UserDTO?>("Address is required", null);
            }

            var fabricClient = new FabricClient();
            var serviceUri = new Uri("fabric:/Web-Shop/UserService");

            var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(serviceUri);
            IUserService proxy = null!;

            foreach (var partition in partitionList)
            {
                var partitionKey = partition.PartitionInformation as Int64RangePartitionInformation;

                if (partitionKey != null)
                {
                    var servicePartitionKey = new ServicePartitionKey(partitionKey.LowKey);

                    proxy = ServiceProxy.Create<IUserService>(serviceUri, servicePartitionKey);
                    break;
                }
            }

            try
            {
                var result = await proxy.ModifyUser(user);
                return new Tuple<string, UserDTO?>(result.Item1, result.Item2);
            }
            catch (Exception ex)
            {
                return new Tuple<string, UserDTO?>("Error in communication with service " + ex.Message, null);
            }

        }

        public async Task<string> RegisterValidator(RegisterDTO newUser)
        {
            if (newUser == null)
            {
                return "User infos are requested";
            }

            if (string.IsNullOrEmpty(newUser.Username))
            {
                return "Username is required";
            }

            if (string.IsNullOrEmpty(newUser.Password))
            {
                return "Password is required";
            }

            if (string.IsNullOrEmpty(newUser.Email))
            {
                return "Email is required";
            }

            if (string.IsNullOrEmpty(newUser.FirstName))
            {
                return "First name is required";
            }

            if (string.IsNullOrEmpty(newUser.LastName))
            {
                return "Last name is required";
            }

            if (string.IsNullOrEmpty(newUser.Address))
            {
                return "Address is required";
            }

            var fabricClient = new FabricClient();
            var serviceUri = new Uri("fabric:/Web-Shop/UserService");

            var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(serviceUri);
            IUserService proxy = null!;

            foreach (var partition in partitionList)
            {
                var partitionKey = partition.PartitionInformation as Int64RangePartitionInformation;

                if (partitionKey != null)
                {
                    var servicePartitionKey = new ServicePartitionKey(partitionKey.LowKey);

                    proxy = ServiceProxy.Create<IUserService>(serviceUri, servicePartitionKey);
                    break;
                }
            }

            try
            {
                var result = await proxy.CreateUser(newUser);
                return result;
            }
            catch (Exception ex)
            {
                return "Error in communication with service " + ex.Message;
            }
        }

        public async Task<string> RemoveFromChartValidator(int id)
        {
            if (id <= 0)
            {
                return "Id must be greather than 0";
            }

            return "Everything is okay";
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
