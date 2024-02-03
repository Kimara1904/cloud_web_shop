using Common.DTOs;
using Common.Interfaces;
using Common.Models;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
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

        public Task<List<Article>> ArticleViewValidator()
        {
            throw new NotImplementedException();
        }

        public void ChartViewValidator()
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

        public async Task<string> LoginValidator(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                return "Username is required";
            }

            if (string.IsNullOrEmpty(password))
            {
                return "Password is required";
            }

            return "Everything is okay";
        }

        public async Task<string> ModifyValidator(UserDTO user)
        {
            if (user == null)
            {
                return "User infos are requested";
            }

            if (string.IsNullOrEmpty(user.Username))
            {
                return "Username is required";
            }

            if (string.IsNullOrEmpty(user.Password))
            {
                return "Password is required";
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                return "Email is required";
            }

            if (string.IsNullOrEmpty(user.FirstName))
            {
                return "First name is required";
            }

            if (string.IsNullOrEmpty(user.LastName))
            {
                return "Last name is required";
            }

            if (string.IsNullOrEmpty(user.Address))
            {
                return "Address is required";
            }

            return "Everything is okay";
        }

        public async Task<string> RegisterValidator(UserDTO newUser)
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

            return "Everything is okay";
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
