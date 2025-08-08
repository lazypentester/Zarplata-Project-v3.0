using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.User.Session
{
    public class LogoutModel
    {
        public string RefreshToken { get; set; }
        public string Fingerprint { get; set; }

        public LogoutModel(string refreshToken, string fingerprint)
        {
            RefreshToken = refreshToken;
            Fingerprint = fingerprint;
        }
    }
}
