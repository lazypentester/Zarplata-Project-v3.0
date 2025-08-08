using CommonModels.User.Models;
using CommonModels.User.Session;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopWPFManagementApp.Services.Collection
{
    public class UserService
    {
        //static string authenticationString = "11186570:60-dayfreetrial";
        //static string base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

        private DateTime? TokenExpiredUtcDateTime { get; set; } = null;

        private List<UserRole>? UserRoles { get; set; } = null;
        private List<UserActions>? UserActions { get; set; } = null;

        private HttpClient HTTP_CONNECTION { get; set; }
        private HubConnection HUB_CONNECTION { get; set; }
        private SystemService SYSTEM_SERVICE { get; set; }

        public UserService(HttpClient HTTP_CONNECTION, HubConnection HUB_CONNECTION, SystemService SYSTEM_SERVICE)
        {
            this.HTTP_CONNECTION = HTTP_CONNECTION;
            this.HUB_CONNECTION = HUB_CONNECTION;
            this.SYSTEM_SERVICE = SYSTEM_SERVICE;

            SetFingerprint();
        }

        public async Task<AuthenticatedUserModel?> AuthenticateUser(NetworkCredential credential, string fingerprint, CancellationToken cancellationToken)
        {
            Uri? endpoint = null;
            HttpResponseMessage? responce = null;
            string? responceContent = null;
            AuthenticatedUserModel? authenticatedUserModel = null;
            /*Dictionary<string, string> pidar = new Dictionary<string, string>()
            {
                { nameof(credential.UserName), credential.UserName },
                { nameof(credential.Password), credential.Password },
                { nameof(Fingerprint), Fingerprint }
            };*/
            AuthenticateUserModel authenticateUserModel = new AuthenticateUserModel(credential.UserName, credential.Password, fingerprint);
            JsonContent requestJsonContent = JsonContent.Create(authenticateUserModel);

            endpoint = new Uri("user/login", UriKind.Relative);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = endpoint,
                Content = requestJsonContent,
            };

            //request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            // remove after
            //request.Headers.Add("Remote_Addr", "192.168.0.1");
            // remove after

            responce = await HTTP_CONNECTION.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

            responceContent = await responce.Content.ReadAsStringAsync();
            authenticatedUserModel = JsonConvert.DeserializeObject<AuthenticatedUserModel>(responceContent);

            endpoint = null;
            responce = null;
            responceContent = null;

            return authenticatedUserModel;
        }
        public async Task<RefreshedSessionModel?> RefreshSessionToken(string refreshtoken, string fingerprint, CancellationToken cancellationToken)
        {
            Uri? endpoint = null;
            HttpResponseMessage? responce = null;
            string? responceContent = null;
            RefreshedSessionModel? refreshedSessionModel = null;
            RefreshSessionModel refreshSessionModel = new RefreshSessionModel(refreshtoken, fingerprint);
            JsonContent requestJsonContent = JsonContent.Create(refreshSessionModel);

            endpoint = new Uri("user/refreshsession", UriKind.Relative);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = endpoint,
                Content = requestJsonContent,
            };

            //request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            // remove after
            //request.Headers.Add("Remote_Addr", "192.168.0.1");
            // remove after

            responce = await HTTP_CONNECTION.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

            responceContent = await responce.Content.ReadAsStringAsync();
            refreshedSessionModel = JsonConvert.DeserializeObject<RefreshedSessionModel>(responceContent);

            endpoint = null;
            responce = null;
            responceContent = null;

            return refreshedSessionModel;
        }
        public async Task<LogoutedModel?> DeleteSessionToken(string accesstoken, string refreshtoken, string fingerprint, CancellationToken cancellationToken)
        {
            Uri? endpoint = null;
            HttpResponseMessage? responce = null;
            string? responceContent = null;
            LogoutedModel? logoutedModel = null;
            LogoutModel logoutModel = new LogoutModel(refreshtoken, fingerprint);
            JsonContent requestJsonContent = JsonContent.Create(logoutModel);

            endpoint = new Uri("user/logout", UriKind.Relative);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = endpoint,
                Content = requestJsonContent,
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);

            //request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            // remove after
            //request.Headers.Add("Remote_Addr", "192.168.0.1");
            // remove after

            responce = await HTTP_CONNECTION.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

            responceContent = await responce.Content.ReadAsStringAsync();
            logoutedModel = JsonConvert.DeserializeObject<LogoutedModel>(responceContent);

            endpoint = null;
            responce = null;
            responceContent = null;

            return logoutedModel;
        }

        public bool CanUserExecuteAction(UserActions action)
        {
            return UserActions?.Contains(action) ?? false;
        }

        public List<UserRole>? GetUserRoles()
        {
            return UserRoles;
        }
        public void SetUserRoles(List<UserRole> userRoles)
        {
            UserRoles = userRoles;
        }

        public List<UserActions>? GetUserActions()
        {
            return UserActions;
        }
        public void SetUserActions(List<UserActions> userActions)
        {
            UserActions = userActions;
        }

        public string? GetAccessToken()
        {
            string? accesstoken = null;
            return accesstoken = SYSTEM_SERVICE.ReadEnvironmentVariable(nameof(accesstoken));
        }
        public void SetAccessToken(string accesstoken)
        {
            SYSTEM_SERVICE.WriteEnvironmentVariable(nameof(accesstoken), accesstoken);
        }

        public string? GetRefreshToken()
        {
            string? refreshtoken = null;
            return refreshtoken = SYSTEM_SERVICE.ReadEnvironmentVariable(nameof(refreshtoken));
        }
        public void SetRefreshToken(string refreshtoken)
        {
            SYSTEM_SERVICE.WriteEnvironmentVariable(nameof(refreshtoken), refreshtoken);
        }

        public string? GetFingerprint()
        {
            string? fingerprint = null;
            return fingerprint = SYSTEM_SERVICE.ReadEnvironmentVariable(nameof(fingerprint));
        }
        public void SetFingerprint()
        {
            string fingerprint = SYSTEM_SERVICE.CreateIdentityKey();
            SYSTEM_SERVICE.WriteEnvironmentVariable(nameof(fingerprint), fingerprint);
        }

        public void SetExpiredTokenDateTime(DateTime expiredDateTime)
        {
            TokenExpiredUtcDateTime = expiredDateTime;
        }
        public bool SessionIsExpired()
        {
            bool isExpired = false;

            try
            {
                if (TokenExpiredUtcDateTime == null || DateTime.UtcNow.CompareTo(TokenExpiredUtcDateTime) > 0)
                {
                    isExpired = true;
                }
            }
            catch { }

            return isExpired;
        }
    }
}
