using CommonModels.Email.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.EmailModels
{
    public class Email
    {
        public string Address { get; set; }
        public string Password { get; set; }
        public ConnectSettings ConnectSettings { get; set; }

        public Email(string address, string password, ConnectSettings connectSettings)
        {
            Address = address;
            Password = password;
            ConnectSettings = connectSettings;
        }
    }
}
