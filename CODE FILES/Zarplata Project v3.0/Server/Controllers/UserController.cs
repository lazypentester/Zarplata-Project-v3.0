using Amazon.Runtime.Internal.Transform;
using CommonModels.Client;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using CommonModels.User.Models;
using CommonModels.User.Others;
using CommonModels.User.Session;
using DnsClient;
using Hashing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Linq;
using Server.Attributes.Authorization;
using Server.Database.Services;
using Server.Hubs;
using Server.Tokens;
using ServerExtensions;
using System.Net;
using static CommonModels.Client.Client;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IOptions<AuthOptions> authOptions;
        private readonly ClientsService clientsService;
        private readonly UsersService usersService;
        private readonly UserSessionService userSessionsService;
        private readonly IHubContext<ClientHub> clientHubContext;

        public UserController(IOptions<AuthOptions> authOptions, ClientsService clientsService, UsersService usersService, UserSessionService userSessionsService, IHubContext<ClientHub> clientHubContext)
        {
            this.authOptions = authOptions;
            this.clientsService = clientsService;
            this.usersService = usersService;
            this.userSessionsService = userSessionsService;
            this.clientHubContext = clientHubContext;
        }

        // POST: api/user/login
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] AuthenticateUserModel authenticateUser)
        {
            List<UserRole> userRoles;
            List<UserActions> userActions;

            // get user ip for tokens and session 
            var ip = HttpContext.GetRemoteIPAddress();
            if (ip == null)
            {
                userRoles = new List<UserRole>();
                userRoles.Add(UserRole.NotAuthenticatedUser);

                userActions = new List<UserActions>();
                userActions.AddRange(UserActionsAndRoles.UserActionsAndRolesCollection.Where(item => item.Key == UserRole.NotAuthenticatedUser).First().Value);
              
                return StatusCode(405, new AuthenticatedUserModel(userRoles, userActions, "", "", DateTime.UtcNow, "Unable to detect your ip address.", false));
            }

            // authenticate
            UserModel? user = await Authenticate(authenticateUser.UserName, authenticateUser.Password);

            if(user == null)
            {
                userRoles = new List<UserRole>();
                userRoles.Add(UserRole.NotAuthenticatedUser);

                userActions = new List<UserActions>();
                userActions.AddRange(UserActionsAndRoles.UserActionsAndRolesCollection.Where(item => item.Key == UserRole.NotAuthenticatedUser).First().Value);

                return Unauthorized(new AuthenticatedUserModel(userRoles, userActions, "", "", DateTime.UtcNow, "Username or password incorrect.", false));
            }

            // Delete user old sessions
            await DeleteOldUserSessions(user.Id!);

            // create tokens
            string SessionToken = "";
            string RefreshSessionToken = "";
            try
            {
                SessionToken = AccessToken.GenerateJWTForUser(user, ip.ToString(), authOptions.Value.UserTokenLifetime, authOptions);
                RefreshSessionToken = Guid.NewGuid().ToString();
            } 
            catch (Exception e)
            {
                userRoles = new List<UserRole>();
                userRoles.Add(UserRole.NotAuthenticatedUser);

                userActions = new List<UserActions>();
                userActions.AddRange(UserActionsAndRoles.UserActionsAndRolesCollection.Where(item => item.Key == UserRole.NotAuthenticatedUser).First().Value);

                return StatusCode(400, new AuthenticatedUserModel(userRoles, userActions, "", "", DateTime.UtcNow, e.Message, false));
            }

            // create session in db
            SessionModel session = new SessionModel()
            {
                UserId = user.Id,
                StartDateTime = DateTime.UtcNow,
                ExpiresDateTime = DateTime.UtcNow.AddSeconds(authOptions.Value.UserRefreshTokenLifetime),
                RefreshToken = RefreshSessionToken,
                Fingerprint = authenticateUser.Fingerprint,
                Ip = ip.ToString(),
            };

            try
            {
                await userSessionsService.CreateAsync(session);
            }
            catch (Exception e)
            {
                userRoles = new List<UserRole>();
                userRoles.Add(UserRole.NotAuthenticatedUser);

                userActions = new List<UserActions>();
                userActions.AddRange(UserActionsAndRoles.UserActionsAndRolesCollection.Where(item => item.Key == UserRole.NotAuthenticatedUser).First().Value);

                return StatusCode(400, new AuthenticatedUserModel(userRoles, userActions, "", "", DateTime.UtcNow, e.Message, false));
            }

            userRoles = new List<UserRole>();
            userRoles.AddRange(user.Roles!);

            userActions = new List<UserActions>();
            foreach(var role in user.Roles!)
            {
                userActions.AddRange(UserActionsAndRoles.UserActionsAndRolesCollection.Where(item => item.Key == role).First().Value);
            }

            return Ok(new AuthenticatedUserModel(userRoles, userActions, SessionToken, RefreshSessionToken, DateTime.UtcNow.AddSeconds(authOptions.Value.UserTokenLifetime), "Authenticated Successfully", true));
        }

        // POST: api/user/register
        [HttpPost]
        [Route("register")]
        [Authorize(Roles = $"{nameof(UserRole.Admin)}")]
        [ControllerAuthorizeIP]
        public async Task<IActionResult> Register([FromBody] RegisterUserModel registerUser)
        {
            UserModel? userMatchByUsername = null;
            try
            {
                userMatchByUsername = await usersService.GetByUsernameAsync(registerUser.Username);
            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);
            }

            if(userMatchByUsername != null)
                return StatusCode(411, $"User with username '{registerUser.Username}' already exist");

            try
            {
                string salt = passwordHash.generateSalt(15);
                string hash = passwordHash.generateHash(registerUser.Password, salt);

                UserModel user = new UserModel()
                {
                    Username = registerUser.Username,
                    Password = new UserPasswordModel()
                    {
                        Hash = hash,
                        Salt = salt
                    },
                    Roles = registerUser.Roles,
                    RegistrationDateTime = DateTime.UtcNow
                };

                await usersService.CreateAsync(user);
            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);
            }

            return Ok(new
            {
                username = registerUser.Username,
            });
        }

        // POST: api/user/refreshsession
        [HttpPost]
        [Route("refreshsession")]
        public async Task<IActionResult> RefreshSession([FromBody] RefreshSessionModel refreshSession)
        {
            List<UserRole> userRoles;
            List<UserActions> userActions;

            // get user ip for tokens and session 
            var ip = HttpContext.GetRemoteIPAddress();
            if (ip == null)
            {
                userRoles = new List<UserRole>();
                userRoles.Add(UserRole.NotAuthenticatedUser);

                userActions = new List<UserActions>();
                userActions.AddRange(UserActionsAndRoles.UserActionsAndRolesCollection.Where(item => item.Key == UserRole.NotAuthenticatedUser).First().Value);

                return StatusCode(405, new RefreshedSessionModel(userRoles, userActions, "", "", DateTime.UtcNow, "Unable to detect your ip address.", false));
            }

            // try to find user session with refresh token from request
            SessionModel? userCurrentSession = await userSessionsService.GetByRefreshTokenAsync(refreshSession.RefreshToken);

            if(userCurrentSession == null)
            {
                userRoles = new List<UserRole>();
                userRoles.Add(UserRole.NotAuthenticatedUser);

                userActions = new List<UserActions>();
                userActions.AddRange(UserActionsAndRoles.UserActionsAndRolesCollection.Where(item => item.Key == UserRole.NotAuthenticatedUser).First().Value);

                return StatusCode(405, new RefreshedSessionModel(userRoles, userActions, "", "", DateTime.UtcNow, "Unable to find your session.", false));
            }

            if (DateTime.UtcNow.CompareTo(userCurrentSession.ExpiresDateTime) > 0)
            {
                await userSessionsService.DeleteByRefreshTokenAsync(userCurrentSession.RefreshToken!);

                userRoles = new List<UserRole>();
                userRoles.Add(UserRole.NotAuthenticatedUser);

                userActions = new List<UserActions>();
                userActions.AddRange(UserActionsAndRoles.UserActionsAndRolesCollection.Where(item => item.Key == UserRole.NotAuthenticatedUser).First().Value);

                return StatusCode(405, new RefreshedSessionModel(userRoles, userActions, "", "", DateTime.UtcNow, "REFRESH_TOKEN_EXPIRED.", false));
            }

            if(refreshSession.Fingerprint != userCurrentSession.Fingerprint)
            {
                await userSessionsService.DeleteByRefreshTokenAsync(userCurrentSession.RefreshToken!);

                userRoles = new List<UserRole>();
                userRoles.Add(UserRole.NotAuthenticatedUser);

                userActions = new List<UserActions>();
                userActions.AddRange(UserActionsAndRoles.UserActionsAndRolesCollection.Where(item => item.Key == UserRole.NotAuthenticatedUser).First().Value);

                return StatusCode(405, new RefreshedSessionModel(userRoles, userActions, "", "", DateTime.UtcNow, "ACCESS DENIED.", false));
            }

            #region Refresh Session

            // get user by session.UserId
            UserModel? user = await usersService.GetAsync(userCurrentSession.UserId!);

            if (user == null)
            {
                userRoles = new List<UserRole>();
                userRoles.Add(UserRole.NotAuthenticatedUser);

                userActions = new List<UserActions>();
                userActions.AddRange(UserActionsAndRoles.UserActionsAndRolesCollection.Where(item => item.Key == UserRole.NotAuthenticatedUser).First().Value);

                return Unauthorized(new RefreshedSessionModel(userRoles, userActions, "", "", DateTime.UtcNow, "User Not find", false));
            }

            // Delete user old sessions
            await DeleteOldUserSessions(userCurrentSession.UserId!);

            // create tokens
            string SessionToken = "";
            string RefreshSessionToken = "";
            try
            {
                SessionToken = AccessToken.GenerateJWTForUser(user, ip.ToString(), authOptions.Value.UserTokenLifetime, authOptions);
                RefreshSessionToken = Guid.NewGuid().ToString();
            }
            catch (Exception e)
            {
                userRoles = new List<UserRole>();
                userRoles.Add(UserRole.NotAuthenticatedUser);

                userActions = new List<UserActions>();
                userActions.AddRange(UserActionsAndRoles.UserActionsAndRolesCollection.Where(item => item.Key == UserRole.NotAuthenticatedUser).First().Value);

                return StatusCode(400, new RefreshedSessionModel(userRoles, userActions, "", "", DateTime.UtcNow, e.Message, false));
            }

            userCurrentSession.ExpiresDateTime = DateTime.UtcNow.AddSeconds(authOptions.Value.UserRefreshTokenLifetime);
            userCurrentSession.RefreshToken = RefreshSessionToken;

            await userSessionsService.UpdateAsync(
                userCurrentSession.Id!,
                new Dictionary<string, object>()
                {
                    { nameof(userCurrentSession.ExpiresDateTime), userCurrentSession.ExpiresDateTime },
                    { nameof(userCurrentSession.RefreshToken), userCurrentSession.RefreshToken }
                }
                );

            #endregion

            userRoles = new List<UserRole>();
            userRoles.AddRange(user.Roles!);

            userActions = new List<UserActions>();
            foreach (var role in user.Roles!)
            {
                userActions.AddRange(UserActionsAndRoles.UserActionsAndRolesCollection.Where(item => item.Key == role).First().Value);
            }

            return Ok(new RefreshedSessionModel(userRoles, userActions, SessionToken, RefreshSessionToken, DateTime.UtcNow.AddSeconds(authOptions.Value.UserTokenLifetime), "Session Refreshed Successfully", true));
        }

        // POST: api/user/logout
        [HttpPost]
        [Route("logout")]
        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.Spectator)}")]
        [ControllerAuthorizeIP]
        public async Task<IActionResult> Logout([FromBody] LogoutModel logoutModel)
        {
            // try to find user session with refresh token from request
            SessionModel? userCurrentSession = await userSessionsService.GetByRefreshTokenAsync(logoutModel.RefreshToken);

            if (userCurrentSession == null)
            {
                return StatusCode(405, new LogoutedModel("Unable to find your session.", false)); // change
            }

            if (DateTime.UtcNow.CompareTo(userCurrentSession.ExpiresDateTime) > 0)
            {
                await userSessionsService.DeleteByRefreshTokenAsync(userCurrentSession.RefreshToken!);

                return StatusCode(405, new LogoutedModel("REFRESH_TOKEN_EXPIRED.", false));
            }

            if (logoutModel.Fingerprint != userCurrentSession.Fingerprint)
            {
                await userSessionsService.DeleteByRefreshTokenAsync(userCurrentSession.RefreshToken!);

                return StatusCode(405, new LogoutedModel("ACCESS DENIED.", false));
            }

            // remove session
            await userSessionsService.DeleteAsync(userCurrentSession.Id!);

            return Ok(new LogoutedModel("Session Successfully Deleted", true));
        }

        private async Task<UserModel?> Authenticate(string username, string password)
        {
            UserModel? user = null;

            try
            {
                UserModel? userMatch = await usersService.GetByUsernameAsync(username);

                if(userMatch != null && passwordHash.verifyPass(password, userMatch.Password!.Salt, userMatch.Password.Hash))
                {
                    user = userMatch;
                }
            }
            catch { }

            return user;
        }
        private async Task DeleteOldUserSessions(string userId)
        {
            try
            {
                // get all user sessions
                List<SessionModel>? allUserSessions = await userSessionsService.GetByUserIdAsync(userId);

                if(allUserSessions != null && allUserSessions.Count > 1)
                {
                    // get old sessions
                    List<string> oldUserSessionIds = new List<string>();

                    foreach(var session in allUserSessions)
                    {
                        if(DateTime.UtcNow.CompareTo(session.ExpiresDateTime) > 0)
                        {
                            oldUserSessionIds.Add(session.Id!);
                        }
                    }

                    // remove old sessions
                    if(oldUserSessionIds.Count > 0)
                    {
                        await userSessionsService.DeleteManyAsync(oldUserSessionIds);
                    }
                }
            }
            catch { }
        }
    }
}
