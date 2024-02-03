using System.Runtime.Serialization;

namespace Common.DTOs
{
    [DataContract]
    public class UserDTO
    {
        [DataMember]
        public string Username { get; set; } = null!;
        [DataMember]
        public string Password { get; set; } = null!;
        [DataMember]
        public string Email { get; set; } = null!;
        [DataMember]
        public string FirstName { get; set; } = null!;
        [DataMember]
        public string LastName { get; set; } = null!;
        [DataMember]
        public DateTime Birthday { get; set; }
        [DataMember]
        public string Address { get; set; } = null!;
    }
}
