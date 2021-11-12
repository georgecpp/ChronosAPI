
using System;

namespace ChronosAPI.Models
{
    public class User
    {
        public int UserId { get; set; }

        public String FirstName{ get; set; }

        public String LastName { get; set; }

        public String Email { get; set; }
        public String Password { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
