using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.User.Models
{
    public class RegisterUserModel
    {
        public RegisterUserModel(string username, string password, UserRole[] roles)
        {
            Username = username;
            Password = password;
            Roles = roles;
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public UserRole[] Roles { get; set; }
    }
}
