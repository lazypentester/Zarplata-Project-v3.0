using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.User.Models
{
    public class AuthenticateUserModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Fingerprint { get; set; }

        public AuthenticateUserModel(string userName, string password, string fingerprint)
        {
            UserName = userName;
            Password = password;
            Fingerprint = fingerprint;
        }
    }
}
