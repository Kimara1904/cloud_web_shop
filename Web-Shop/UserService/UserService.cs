using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Common.DTOs;
using Common.Interfaces;
using Common.Models;
using Common.Repository;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace UserService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class UserService : StatefulService, IUserService
    {

        public UserService(StatefulServiceContext context)
            : base(context)
        { }

        private async Task UpdateState()
        {
            string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

            TableServiceClient tableServiceClient = new(connectionString);

            TableItem tableItem = await tableServiceClient.CreateTableIfNotExistsAsync("User");
            TableClient userClient = tableServiceClient.GetTableClient("User");

            var stateManager = this.StateManager;
            var userDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, User>>("userDictionary");

            using var transaction = stateManager.CreateTransaction();
            var operationData = new DataOperations<UserData>(userClient);

            await foreach (var user in operationData.RetrieveAllAsync("User"))
            {
                var userVal = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Password = user.Password,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Birthday = user.Birthday,
                    Address = user.Address
                };
                await userDictionary.AddOrUpdateAsync(transaction, user.Id, userVal, (k, v) => userVal);
            }
            await transaction.CommitAsync();
        }

        public async Task<string> CreateUser(RegisterDTO userDTO)
        {
            var stateManager = this.StateManager;
            var userDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, User>>("userDictionary");

            using var transaction = stateManager.CreateTransaction();
            var userEnumerator = (await userDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();

            while (await userEnumerator.MoveNextAsync(CancellationToken.None))
            {
                var current = userEnumerator.Current;

                if (current.Value.Username.Equals(userDTO.Username) || current.Value.Email.Equals(userDTO.Email))
                {
                    return $"User with username: {userDTO} already exists";
                }
            }

            string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

            TableServiceClient tableServiceClient = new(connectionString);

            TableItem tableItem = await tableServiceClient.CreateTableIfNotExistsAsync("User");
            TableClient UserClient = tableServiceClient.GetTableClient("User");

            var operationData = new DataOperations<UserData>(UserClient);

            var newUser = new UserData(new User
            {
                Username = userDTO.Username,
                Password = userDTO.Password,
                Email = userDTO.Email,
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                Birthday = userDTO.Birthday,
                Address = userDTO.Address
            });

            try
            {
                await operationData.AddAsync(newUser);
                return "Successfully created new user";
            }
            catch (Exception ex)
            {
                return $"Error in creation with service {ex.Message}";
            }
        }

        public async Task<UserDTO?> GetUser(string username, string password)
        {
            var stateManager = this.StateManager;
            var userDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, User>>("userDictionary");

            using var transaction = stateManager.CreateTransaction();
            var userEnumerator = (await userDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();

            while (await userEnumerator.MoveNextAsync(CancellationToken.None))
            {
                var user = userEnumerator.Current;

                if (user.Value.Username.Equals(username) && user.Value.Password == password)
                {
                    var returnVal = new UserDTO
                    {
                        Id = user.Value.Id,
                        Username = user.Value.Username,
                        Password = user.Value.Password,
                        Email = user.Value.Email,
                        FirstName = user.Value.FirstName,
                        LastName = user.Value.LastName,
                        Birthday = user.Value.Birthday,
                        Address = user.Value.Address
                    };

                    return returnVal;
                }
            }

            return null;
        }

        public async Task<Tuple<string, UserDTO?>> ModifyUser(ModifyDTO userDTO)
        {
            var stateManager = this.StateManager;
            var userDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, User>>("userDictionary");

            using var transaction = stateManager.CreateTransaction();
            var user = await userDictionary.TryGetValueAsync(transaction, userDTO.Id);

            if (user.Value == null)
            {
                return new Tuple<string, UserDTO?>($"There is no user with id: {userDTO.Id}", null);
            }

            string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

            TableServiceClient tableServiceClient = new(connectionString);

            TableItem tableItem = await tableServiceClient.CreateTableIfNotExistsAsync("User");
            TableClient UserClient = tableServiceClient.GetTableClient("User");

            var operationData = new DataOperations<UserData>(UserClient);

            var newUserInfo = new UserData(new User
            {
                Id = userDTO.Id,
                Username = userDTO.Username,
                Password = userDTO.Password,
                Email = userDTO.Email,
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                Birthday = userDTO.Birthday,
                Address = userDTO.Address
            });

            try
            {
                await operationData.Modify(newUserInfo);
                var returnUser = new UserDTO
                {
                    Id = userDTO.Id,
                    Username = userDTO.Username,
                    Password = userDTO.Password,
                    Email = userDTO.Email,
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    Birthday = userDTO.Birthday,
                    Address = userDTO.Address
                };

                return new Tuple<string, UserDTO?>("Successfully modified user", returnUser);
            }
            catch (Exception ex)
            {
                return new Tuple<string, UserDTO?>($"Error in modify with service {ex.Message}", null);
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await UpdateState();

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}
