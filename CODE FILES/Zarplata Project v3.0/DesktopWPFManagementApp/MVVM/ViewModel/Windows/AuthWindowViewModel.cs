using DesktopWPFManagementApp.Commands;
using DesktopWPFManagementApp.MVVM.View.CustomControls.Panels;
using DesktopWPFManagementApp.MVVM.ViewModel.Core;
using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls;
using DesktopWPFManagementApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DesktopWPFManagementApp.MVVM.ViewModel.Windows
{
    internal class AuthWindowViewModel : ViewModelBase
    {
        #region Enums
        public enum ServerConnectStatusEnum
        {
            NotConnected,
            SuccessfullyConnected,
            ConnectError
        }
        #endregion

        #region properties
        private bool isViewVisible = true;
        public bool IsViewVisible
        {
            get { return isViewVisible; }
            set
            {
                isViewVisible = value;
                OnPropertyChanged();
            }
        }

        private ViewModelBase currentChildView;
        public ViewModelBase CurrentChildView
        {
            get { return currentChildView; }
            set
            {
                currentChildView = value;
                OnPropertyChanged();
            }
        }

        private string serverConnectStatus = ServerConnectStatusEnum.NotConnected.ToString();
        public string ServerConnectStatus
        {
            get { return serverConnectStatus; }
            set
            {
                serverConnectStatus = value;
                OnPropertyChanged();
            }
        }


        private AuthPanelUserControlViewModel authPanelUserControlViewModel;
        private ConnectErrorUserControlViewModel connectErrorUserControlViewModel;
        private LoadPanelUserControlViewModel loadPanelUserControlViewModel;
        #endregion

        #region commands
        #endregion

        public AuthWindowViewModel()
        {
            #region Init Child Views And CurrentView
            authPanelUserControlViewModel = new AuthPanelUserControlViewModel();
            connectErrorUserControlViewModel = new ConnectErrorUserControlViewModel();
            loadPanelUserControlViewModel = new LoadPanelUserControlViewModel();

            currentChildView = loadPanelUserControlViewModel;
            #endregion

            #region Init Commands

            #endregion

            #region init event handlers
            authPanelUserControlViewModel.AuthCommand.AccountSuccessfullyAuthorized += AuthCommand_AccountSuccessfullyAuthorized;
            authPanelUserControlViewModel.AuthCommand.AccountAuthorizationFailed += AuthCommand_AccountAuthorizationFailed;
            authPanelUserControlViewModel.AuthCommand.ConnectionError += AuthCommand_ConnectionError;

            connectErrorUserControlViewModel.ReconnectToServerCommand.SuccessfullyReconnected += ReconnectToServerCommand_SuccessfullyReconnected;
            connectErrorUserControlViewModel.ReconnectToServerCommand.ReconnectFailed += ReconnectToServerCommand_ReconnectFailed;
            #endregion

            // try to connect to server
            FirstConnectToServer();
        }

        private void AuthCommand_AccountSuccessfullyAuthorized()
        {
            authPanelUserControlViewModel.Username = "";
            authPanelUserControlViewModel.Password = "";
            IsViewVisible = false;
        }
        private void AuthCommand_AccountAuthorizationFailed(string msg)
        {
            authPanelUserControlViewModel.AuthFailStatusText = msg;
        }
        private void AuthCommand_ConnectionError()
        {
            CurrentChildView = connectErrorUserControlViewModel;
        }

        private void ReconnectToServerCommand_SuccessfullyReconnected()
        {
            ServerConnectStatus = ServerConnectStatusEnum.SuccessfullyConnected.ToString();
            CurrentChildView = authPanelUserControlViewModel;
        }
        private void ReconnectToServerCommand_ReconnectFailed()
        {
            ServerConnectStatus = ServerConnectStatusEnum.NotConnected.ToString();
            ServerConnectStatus = ServerConnectStatusEnum.ConnectError.ToString();
        }

        private async void FirstConnectToServer()
        {
            CancellationTokenSource cancellationTokenSourceConnectToServer = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            CancellationTokenSource cancellationTokenSourceConnectToServerHub = new CancellationTokenSource(TimeSpan.FromSeconds(8));

            try
            {
                CurrentChildView = loadPanelUserControlViewModel;

                await ServiceStorage.ServerConnectService.TestServerConnection(cancellationTokenSourceConnectToServer.Token);
                await ServiceStorage.ServerConnectService.TestServerHubConnection(cancellationTokenSourceConnectToServerHub.Token);

                ServerConnectStatus = ServerConnectStatusEnum.SuccessfullyConnected.ToString();

                CurrentChildView = authPanelUserControlViewModel;
            }
            catch (Exception e)
            {
                ServerConnectStatus = ServerConnectStatusEnum.ConnectError.ToString();
                // set except to textbox.. in connectErrorUserControlViewModel ?
                CurrentChildView = connectErrorUserControlViewModel;
            }
        }
    }
}
