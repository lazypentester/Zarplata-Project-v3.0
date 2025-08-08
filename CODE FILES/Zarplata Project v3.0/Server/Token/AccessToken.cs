using CommonModels.Client;
using CommonModels.Client.Models;
using CommonModels.User.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Server.Tokens
{
    public static class AccessToken
    {
        public static string GenerateJWTForBotClient(ModelClient client, IOptions<AuthOptions> authOptions)
        {
            var authParams = authOptions.Value;

            var securitykey = authParams.GetSymmetricSecurityKey();
            var creditials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, client.ID!),
                new Claim(ClaimTypes.NameIdentifier, client.ID!),
                new Claim("ip", client.IP!),
                new Claim("role", client.Role!.Value.ToString())
            };

            var token = new JwtSecurityToken(
                authParams.Issuer,
                authParams.Audience,
                claims,
                expires: DateTime.Now.AddSeconds(authParams.TokenLifetime),
                signingCredentials: creditials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static string GenerateJWTForBotClient(ModelClient client, double expires, IOptions<AuthOptions> authOptions)
        {
            var authParams = authOptions.Value;

            var securitykey = authParams.GetSymmetricSecurityKey();
            var creditials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, client.ID!),
                new Claim(ClaimTypes.NameIdentifier, client.ID!),
                new Claim("ip", client.IP!),
                new Claim("role", client.Role!.Value.ToString())
            };

            DateTime? time = null;
            if (expires != 0)
                time = DateTime.Now.AddSeconds(expires);

            var token = new JwtSecurityToken(
                authParams.Issuer,
                authParams.Audience,
                claims,
                expires: time,
                signingCredentials: creditials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static string GenerateJWTForUser(UserModel user, string ip, double expires, IOptions<AuthOptions> authOptions)
        {
            var authParams = authOptions.Value;

            var securitykey = authParams.GetSymmetricSecurityKey();
            var creditials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id!),
                new Claim(ClaimTypes.NameIdentifier, user.Id!),
                new Claim(ClaimTypes.Name, user.Username!),
                new Claim("ip", ip)
            };

            foreach (var role in user.Roles!)
            {
                claims.Add(new Claim("role", role.ToString()));
            }

            DateTime? time = null;
            if (expires != 0)
                time = DateTime.Now.AddSeconds(expires);

            var token = new JwtSecurityToken(
                authParams.Issuer,
                authParams.Audience,
                claims,
                expires: time,
                signingCredentials: creditials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
