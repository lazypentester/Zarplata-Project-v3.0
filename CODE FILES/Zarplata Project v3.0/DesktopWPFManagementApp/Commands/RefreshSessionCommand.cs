using CommonModels.User.Models;
using CommonModels.User.Session;
using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls;
using DesktopWPFManagementApp.MVVM.ViewModel.Windows;
using DesktopWPFManagementApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.AuthPanelUserControlViewModel;
using static DesktopWPFManagementApp.MVVM.ViewModel.Windows.MainWindowViewModel;

namespace DesktopWPFManagementApp.Commands
{
    internal class RefreshSessionCommand : RelayCommand
    {
        // properties
        private MainWindowViewModel mainWindowVM;

        // events
        public event Action SessionSuccessfullyRefreshed = () => { };
        public event Action<string> RefreshSessionFailed = (string msg) => { };
        public event Action ConnectionError = () => { };

        public RefreshSessionCommand(MainWindowViewModel mainWindowVM)
        {
            this.mainWindowVM = mainWindowVM;
        }

        public override bool CanExecute(object? parameter)
        {
            return mainWindowVM.TokenRefreshStatus == TokenRefreshStatusEnum.TokenIsInvalid && mainWindowVM.IsRefreshSessionPopupPanelOpen && base.CanExecute(parameter);
        }

        public override async void Execute(object? parameter)
        {
            mainWindowVM.TokenRefreshStatus = TokenRefreshStatusEnum.TokenIsRefreshing;

            // logic
            CancellationTokenSource cancellationTokenSourceRefreshSession = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            string? accesstoken = null;
            string? refreshtoken = null;
            string? fingerprint = null;
            bool canTryToGetRefreshtoken = false;
            bool canTryToGetFingerprint = false;
            bool canTryToRefreshSession = false;
            RefreshedSessionModel? refreshedSession = null;
            string refreshSessionFailedText = "Произошла непредвиденная ошибка.";

            // try to get accesstoken
            try
            {
                try
                {
                    accesstoken = ServiceStorage.UserService.GetAccessToken();
                    if (accesstoken != null && !String.IsNullOrEmpty(accesstoken))
                    {
                        canTryToGetRefreshtoken = true;
                    }
                }
                catch { }
            }
            catch { }

            // try to get refreshtoken
            if (canTryToGetRefreshtoken)
            {
                try
                {
                    try
                    {
                        refreshtoken = ServiceStorage.UserService.GetRefreshToken();
                        if (refreshtoken != null && !String.IsNullOrEmpty(refreshtoken))
                        {
                            canTryToGetFingerprint = true;
                        }
                    }
                    catch { }
                }
                catch { } 
            }

            // try to get fingerprint
            if (canTryToGetFingerprint)
            {
                try
                {
                    fingerprint = ServiceStorage.UserService.GetFingerprint();
                    if (fingerprint != null && !String.IsNullOrEmpty(fingerprint))
                    {
                        canTryToRefreshSession = true;
                    }
                    else
                    {
                        ServiceStorage.UserService.SetFingerprint();
                        fingerprint = ServiceStorage.UserService.GetFingerprint();
                        if (fingerprint != null && !String.IsNullOrEmpty(fingerprint))
                        {
                            canTryToRefreshSession = true;
                        }
                        else
                        {
                            refreshSessionFailedText = "Не удалось получить fingerprint.";
                        }
                    }
                }
                catch { }
            }

            // try to auth user
            if (canTryToRefreshSession)
            {
                try
                {
                    refreshedSession = await ServiceStorage.UserService.RefreshSessionToken(refreshtoken!, fingerprint!, cancellationTokenSourceRefreshSession.Token);

                    if (refreshedSession != null && refreshedSession.RefreshStatus == true)
                    {
                        // update tokens from refreshedSession to thread and reconnect to server hub
                        try
                        {
                            // update access token
                            /*GenericIdentity AccessTokenIdentity = new GenericIdentity(refreshedSession.SessionAccessToken);
                            Thread.CurrentPrincipal = new GenericPrincipal(AccessTokenIdentity, null);*/
                            ServiceStorage.UserService.SetAccessToken(refreshedSession.SessionAccessToken);

                            // update refresh token
                            ServiceStorage.UserService.SetRefreshToken(refreshedSession.SessionRefreshToken);

                            // save user roles
                            ServiceStorage.UserService.SetUserRoles(refreshedSession.UserRoles);

                            // save user actions
                            ServiceStorage.UserService.SetUserActions(refreshedSession.UserActions);

                            // save session expired date time
                            ServiceStorage.UserService.SetExpiredTokenDateTime(refreshedSession.TokenExpiredUtcDateTime);

                            // reconnect to server hub
                            CancellationTokenSource cancellationTokenSourceDisconnect = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                            CancellationTokenSource cancellationTokenSourceConnect = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                            await ServiceStorage.ServerConnectService.ReconnectToServerHub(cancellationTokenSourceDisconnect.Token, cancellationTokenSourceConnect.Token);
                        }
                        catch { }

                        SessionSuccessfullyRefreshed?.Invoke();
                    }
                    else if (refreshedSession != null && refreshedSession.RefreshStatus == false)
                    {
                        RefreshSessionFailed?.Invoke(refreshedSession.RefreshStatusText);
                    }
                    else if (refreshedSession == null)
                    {
                        refreshSessionFailedText = "Произошла непредвиденная ошибка.";
                        RefreshSessionFailed?.Invoke(refreshSessionFailedText);
                    }
                }
                catch
                {
                    ConnectionError?.Invoke();
                }
            }
            else
            {
                RefreshSessionFailed?.Invoke(refreshSessionFailedText);
            }
        }
    }
}
