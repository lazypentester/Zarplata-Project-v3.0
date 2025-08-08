using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Server.Tokens
{
    public class AuthOptions
    {
        public string Issuer { get; set; } = "";

        public string Audience { get; set; } = "";

        public string Secret { get; set; } = "";

        public double TokenLifetime { get; set; } // secs
        public double BotClientTokenLifetime { get; set; } // secs
        public double UserTokenLifetime { get; set; } // secs
        public double UserRefreshTokenLifetime { get; set; } // secs

        public SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret));
        }
    }
}
