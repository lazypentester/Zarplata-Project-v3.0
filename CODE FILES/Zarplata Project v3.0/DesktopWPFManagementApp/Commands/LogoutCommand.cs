using CommonModels.User.Session;
using DesktopWPFManagementApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopWPFManagementApp.Commands
{
    internal class LogoutCommand : RelayCommand
    {
        // properties

        // events
        public event Action SessionSuccessfullyDeleted = () => { };
        public event Action<string> DeleteSessionFailed = (string msg) => { };
        public event Action ConnectionError = () => { };

        public LogoutCommand()
        {
        }

        public override bool CanExecute(object? parameter)
        {
            return base.CanExecute(parameter);
        }

        public override async void Execute(object? parameter)
        {
            // logic
            CancellationTokenSource cancellationTokenSourceDeleteSession = new CancellationTokenSource(TimeSpan.FromSeconds(115));
            string? accesstoken = null;
            string? refreshtoken = null;
            string? fingerprint = null;
            bool canTryToGetRefreshtoken = false;
            bool canTryToGetFingerprint = false;
            bool canTryToDeleteSession = false;
            LogoutedModel? logoutedModel = null;
            string deleteSessionFailedText = "Произошла непредвиденная ошибка.";

            // try to get accesstoken
            try
            {
                try
                {
                    accesstoken = ServiceStorage.UserService.GetAccessToken();
                    if (accesstoken != null)
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
                        if (refreshtoken != null)
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
                    if (fingerprint != null)
                    {
                        canTryToDeleteSession = true;
                    }
                    else
                    {
                        ServiceStorage.UserService.SetFingerprint();
                        fingerprint = ServiceStorage.UserService.GetFingerprint();
                        if (fingerprint != null)
                        {
                            canTryToDeleteSession = true;
                        }
                        else
                        {
                            deleteSessionFailedText = "Не удалось получить fingerprint.";
                        }
                    }
                }
                catch { }
            }

            // try to auth user
            if (canTryToDeleteSession)
            {
                try
                {
                    logoutedModel = await ServiceStorage.UserService.DeleteSessionToken(accesstoken!, refreshtoken!, fingerprint!, cancellationTokenSourceDeleteSession.Token);

                    if (logoutedModel != null && logoutedModel.LogoutStatus == true)
                    {
                        // delete tokens and disconnect from server hub
                        try
                        {
                            // delete access token
                            ServiceStorage.UserService.SetAccessToken("");

                            // delete refresh token
                            ServiceStorage.UserService.SetRefreshToken("");

                            // disconnect from server hub
                            CancellationTokenSource cancellationTokenSourceDisconnect = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                            await ServiceStorage.ServerConnectService.DisconnectFromServerHub(cancellationTokenSourceDisconnect.Token);
                        }
                        catch { }

                        //authPanelUserControlVM.AuthStatus = AuthStatusEnum.ReadyToAuth.ToString();
                        SessionSuccessfullyDeleted?.Invoke();
                    }
                    else if (logoutedModel != null && logoutedModel.LogoutStatus == false)
                    {
                        //authPanelUserControlVM.AuthStatus = AuthStatusEnum.FailAuth.ToString();
                        DeleteSessionFailed?.Invoke(logoutedModel.LogoutStatusText);
                    }
                    else if (logoutedModel == null)
                    {
                        //authPanelUserControlVM.AuthStatus = AuthStatusEnum.FailAuth.ToString();
                        deleteSessionFailedText = "Произошла непредвиденная ошибка.";
                        DeleteSessionFailed?.Invoke(deleteSessionFailedText);
                    }
                }
                catch
                {
                    //authPanelUserControlVM.AuthStatus = AuthStatusEnum.ReadyToAuth.ToString();
                    ConnectionError?.Invoke();
                }
            }
            else
            {
                //authPanelUserControlVM.AuthStatus = AuthStatusEnum.FailAuth.ToString();
                DeleteSessionFailed?.Invoke(deleteSessionFailedText);
            }
        }
    }
}
