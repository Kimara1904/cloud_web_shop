using Azure;
using Azure.Data.Tables;
using Common.Models;

namespace Common.Repository
{
    public class UserData : ITableEntity
    {
        public UserData(User user)
        {
            if (user.Id == 0)
            {
                long id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                this.Id = id;
                RowKey = id.ToString();
            }
            else
            {
                this.Id = user.Id;
                RowKey = user.Id.ToString();
            }
            this.Username = user.Username;
            this.Password = user.Password;
            this.Email = user.Email;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.Birthday = DateTime.SpecifyKind(user.Birthday, DateTimeKind.Utc);
            this.Address = user.Address;
            PartitionKey = "User";
        }

        public UserData() { }

        public long Id { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime Birthday { get; set; }
        public string Address { get; set; } = null!;

        #region ITableEntity implementation
        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        #endregion
    }
}
