using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.User.Models
{
    public class AuthenticatedUserModel
    {
        public List<UserRole> UserRoles { get; set; }
        public List<UserActions> UserActions { get; set; }
        public string SessionAccessToken { get; set; }
        public string SessionRefreshToken { get; set; }
        public DateTime TokenExpiredUtcDateTime { get; set; }
        public string AuthStatusText { get; set; }
        public bool AuthStatus { get; set; }

        public AuthenticatedUserModel(List<UserRole> userRoles, List<UserActions> userActions, string sessionAccessToken, string sessionRefreshToken, DateTime tokenExpiredUtcDateTime, string authStatusText, bool authStatus)
        {
            UserRoles = userRoles;
            UserActions = userActions;
            SessionAccessToken = sessionAccessToken;
            SessionRefreshToken = sessionRefreshToken;
            TokenExpiredUtcDateTime = tokenExpiredUtcDateTime;
            AuthStatusText = authStatusText;
            AuthStatus = authStatus;
        }
    }
}
