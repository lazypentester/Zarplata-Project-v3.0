using CommonModels.User.Models;
using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls;
using DesktopWPFManagementApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.AuthPanelUserControlViewModel;

namespace DesktopWPFManagementApp.Commands
{
    internal class AuthCommand : RelayCommand
    {
        // properties
        private AuthPanelUserControlViewModel authPanelUserControlVM;

        // events
        public event Action AccountSuccessfullyAuthorized = () => { };
        public event Action<string> AccountAuthorizationFailed = (string msg) => { };
        public event Action ConnectionError = () => { };

        public AuthCommand(AuthPanelUserControlViewModel authPanelUserControlVM)
        {
            this.authPanelUserControlVM = authPanelUserControlVM;
        }

        public override bool CanExecute(object? parameter)
        {
            return (authPanelUserControlVM.AuthStatus == AuthStatusEnum.ReadyToAuth.ToString() || authPanelUserControlVM.AuthStatus == AuthStatusEnum.FailAuth.ToString()) && base.CanExecute(parameter);
        }

        public override async void Execute(object? parameter)
        {
            authPanelUserControlVM.AuthStatus = AuthStatusEnum.IsAuth.ToString();

            // logic
            CancellationTokenSource cancellationTokenSourceAuthUser = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            NetworkCredential? credential = null;
            string? fingerprint = null;
            bool canTryToGetFingerprint = false;
            bool canTryToAuth = false;
            AuthenticatedUserModel? authenticatedUser = null;
            string accountAuthorizationFailedText = "Произошла непредвиденная ошибка.";

            // try to caste auth credential
            try
            {
                credential = (parameter as NetworkCredential);
                if (credential != null)
                {
                    canTryToGetFingerprint = true;
                }
                else
                {
                    accountAuthorizationFailedText = "Не удалось получить данные для входа.";
                }
            }
            catch { }

            // try to get fingerprint
            if (canTryToGetFingerprint)
            {
                try
                {
                    fingerprint = ServiceStorage.UserService.GetFingerprint();
                    if (fingerprint != null)
                    {
                        canTryToAuth = true;
                    }
                    else
                    {
                        ServiceStorage.UserService.SetFingerprint();
                        fingerprint = ServiceStorage.UserService.GetFingerprint();
                        if (fingerprint != null)
                        {
                            canTryToAuth = true;
                        }
                        else
                        {
                            accountAuthorizationFailedText = "Не удалось получить fingerprint.";
                        }
                    }
                }
                catch { }
            }

            // try to auth user
            if (canTryToAuth)
            {
                try
                {
                    authenticatedUser = await ServiceStorage.UserService.AuthenticateUser(credential!, fingerprint!, cancellationTokenSourceAuthUser.Token);

                    if (authenticatedUser != null && authenticatedUser.AuthStatus == true)
                    {
                        // save tokens from authenticatedUser to thread(dont use it) and reconnect to server hub
                        try
                        {
                            // save access token
                            ServiceStorage.UserService.SetAccessToken(authenticatedUser.SessionAccessToken);

                            // save refresh token
                            ServiceStorage.UserService.SetRefreshToken(authenticatedUser.SessionRefreshToken);

                            // save user roles
                            ServiceStorage.UserService.SetUserRoles(authenticatedUser.UserRoles);

                            // save user actions
                            ServiceStorage.UserService.SetUserActions(authenticatedUser.UserActions);

                            // save session expired date time
                            ServiceStorage.UserService.SetExpiredTokenDateTime(authenticatedUser.TokenExpiredUtcDateTime);

                            // reconnect to server hub
                            CancellationTokenSource cancellationTokenSourceDisconnect = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                            CancellationTokenSource cancellationTokenSourceConnect = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                            await ServiceStorage.ServerConnectService.ReconnectToServerHub(cancellationTokenSourceDisconnect.Token, cancellationTokenSourceConnect.Token);
                        }
                        catch { }

                        authPanelUserControlVM.AuthStatus = AuthStatusEnum.ReadyToAuth.ToString();

                        AccountSuccessfullyAuthorized?.Invoke();
                    }
                    else if(authenticatedUser != null && authenticatedUser.AuthStatus == false)
                    {
                        authPanelUserControlVM.AuthStatus = AuthStatusEnum.FailAuth.ToString();
                        AccountAuthorizationFailed?.Invoke(authenticatedUser.AuthStatusText);
                    }
                    else if(authenticatedUser == null)
                    {
                        authPanelUserControlVM.AuthStatus = AuthStatusEnum.FailAuth.ToString();
                        accountAuthorizationFailedText = "Произошла непредвиденная ошибка.";
                        AccountAuthorizationFailed?.Invoke(accountAuthorizationFailedText);
                    }
                }
                catch
                {
                    authPanelUserControlVM.AuthStatus = AuthStatusEnum.ReadyToAuth.ToString();
                    ConnectionError?.Invoke();
                }
            }
            else
            {
                authPanelUserControlVM.AuthStatus = AuthStatusEnum.FailAuth.ToString(); 
                AccountAuthorizationFailed?.Invoke(accountAuthorizationFailedText);
            }
        }
    }
}
