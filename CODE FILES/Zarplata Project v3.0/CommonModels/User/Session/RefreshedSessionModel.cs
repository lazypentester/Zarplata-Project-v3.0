using CommonModels.User.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.User.Session
{
    public class RefreshedSessionModel
    {
        public List<UserRole> UserRoles { get; set; }
        public List<UserActions> UserActions { get; set; }
        public string SessionAccessToken { get; set; }
        public string SessionRefreshToken { get; set; }
        public DateTime TokenExpiredUtcDateTime { get; set; }
        public string RefreshStatusText { get; set; }
        public bool RefreshStatus { get; set; }

        public RefreshedSessionModel(List<UserRole> userRoles, List<UserActions> userActions, string sessionAccessToken, string sessionRefreshToken, DateTime tokenExpiredUtcDateTime, string refreshStatusText, bool refreshStatus)
        {
            UserRoles = userRoles;
            UserActions = userActions;
            SessionAccessToken = sessionAccessToken;
            SessionRefreshToken = sessionRefreshToken;
            TokenExpiredUtcDateTime = tokenExpiredUtcDateTime;
            RefreshStatusText = refreshStatusText;
            RefreshStatus = refreshStatus;
        }
    }
}
