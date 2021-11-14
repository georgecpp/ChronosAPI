using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChronosAPI.Models
{
    public class AuthenticateResponse
    {
        public int UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string Token { get; set; }

        public AuthenticateResponse(User user, string token)
        {
            UserId = user.UserId;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            DateOfBirth = user.DateOfBirth;
            CreatedAt = user.CreatedAt;
            Token = token;
        }
    }
}
