using DesktopWPFManagementApp.Commands;
using DesktopWPFManagementApp.MVVM.ViewModel.Core;
using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls;
using DesktopWPFManagementApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopWPFManagementApp.MVVM.ViewModel.Windows
{
    internal class MainWindowViewModel : ViewModelBase
    {
        #region Enums
        public enum ServerConnectStatusEnum
        {
            Connected,
            NotConnected,
            Undefined
        }

        public enum MenuPanelItemEnum
        {
            AnalyticsPanel,
            TasksPanel,
            BotsPanel,
            SettingsPanel,
            ProfilePanel
        }

        public enum TokenRefreshStatusEnum
        {
            TokenIsValid,
            TokenIsInvalid,
            TokenIsRefreshing,
            TokenRefreshFail
        }
        #endregion

        #region Properties

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

        private string serverConnectStatus = ServerConnectStatusEnum.Connected.ToString();
        public string ServerConnectStatus
        {
            get { return serverConnectStatus; }
            set
            {
                serverConnectStatus = value;
                OnPropertyChanged();
            }
        }

        private MenuPanelItemEnum currentSelectedMenuPanelItem = MenuPanelItemEnum.AnalyticsPanel;
        public MenuPanelItemEnum CurrentSelectedMenuPanelItem
        {
            get { return currentSelectedMenuPanelItem; }
            set { currentSelectedMenuPanelItem = value; }
        }

        #region Refresh Session Popup

        public Timer CheckSessionOnExpireTimer { get; set; }

        private bool isRefreshSessionPopupPanelOpen = false;
        public bool IsRefreshSessionPopupPanelOpen
        {
            get { return isRefreshSessionPopupPanelOpen; }
            set 
            { 
                isRefreshSessionPopupPanelOpen = value;
                OnPropertyChanged();
            }
        }

        private TokenRefreshStatusEnum tokenRefreshStatus = TokenRefreshStatusEnum.TokenIsValid;
        public TokenRefreshStatusEnum TokenRefreshStatus
        {
            get { return tokenRefreshStatus; }
            set 
            { 
                tokenRefreshStatus = value;
                OnPropertyChanged();
            }
        }

        #endregion

        // Fields
        private AnalyticsPanelUserControlViewModel analyticsPanelUserControlVM;
        private TasksPanelUserControlViewModel tasksPanelUserControlVM;
        private BotsPanelUserControlViewModel botsPanelUserControlVM;
        private SettingsPanelUserControlViewModel settingsPanelUserControlVM;
        private ProfilePanelUserControlViewModel profilePanelUserControlVM;
        private ConnectErrorUserControlViewModel connectErrorUserControlViewModel;
        private LoadPanelUserControlViewModel loadPanelUserControlViewModel;

        #endregion

        #region Commands
        public RelayCommand openAnalyticsPanelCommand { get; }
        public RelayCommand openTasksPanelCommand { get; }
        public RelayCommand openBotsPanelCommand { get; }
        public RelayCommand openSettingsPanelCommand { get; }
        public RelayCommand openProfilePanelCommand { get; }

        public RefreshSessionCommand refreshSessionMainCommand { get; }
        public RelayCommand openRefreshSessionPanelCommand { get; }
        public RelayCommand closeRefreshSessionPanelCommand { get; }
        #endregion

        public MainWindowViewModel()
        {
            #region Init Child Views And CurrentView
            analyticsPanelUserControlVM = new AnalyticsPanelUserControlViewModel();
            tasksPanelUserControlVM = new TasksPanelUserControlViewModel();
            botsPanelUserControlVM = new BotsPanelUserControlViewModel();
            settingsPanelUserControlVM = new SettingsPanelUserControlViewModel();
            profilePanelUserControlVM = new ProfilePanelUserControlViewModel();
            connectErrorUserControlViewModel = new ConnectErrorUserControlViewModel();
            loadPanelUserControlViewModel = new LoadPanelUserControlViewModel();

            currentChildView = loadPanelUserControlViewModel;
            #endregion

            #region Init Commands
            openAnalyticsPanelCommand = new RelayCommand(execute => 
            { 
                CurrentChildView = analyticsPanelUserControlVM;
                CurrentSelectedMenuPanelItem = MenuPanelItemEnum.AnalyticsPanel;
            }, 
            canExecute => 
            { 
                return ServerConnectStatus == ServerConnectStatusEnum.Connected.ToString(); 
            });
            openTasksPanelCommand = new RelayCommand(execute => 
            {
                CurrentChildView = tasksPanelUserControlVM;
                CurrentSelectedMenuPanelItem = MenuPanelItemEnum.TasksPanel;
                tasksPanelUserControlVM.LoadLastOpenPanel();
            }, 
            canExecute => 
            {
                return ServerConnectStatus == ServerConnectStatusEnum.Connected.ToString();
            });
            openBotsPanelCommand = new RelayCommand(execute => 
            { 
                CurrentChildView = botsPanelUserControlVM;
                CurrentSelectedMenuPanelItem = MenuPanelItemEnum.BotsPanel;
                botsPanelUserControlVM.LoadBotsCollection();
            }, 
            canExecute => 
            {
                return ServerConnectStatus == ServerConnectStatusEnum.Connected.ToString();
            });
            openSettingsPanelCommand = new RelayCommand(execute => 
            { 
                CurrentChildView = settingsPanelUserControlVM;
                CurrentSelectedMenuPanelItem = MenuPanelItemEnum.SettingsPanel;
            }, 
            canExecute => 
            {
                return ServerConnectStatus == ServerConnectStatusEnum.Connected.ToString();
            });
            openProfilePanelCommand = new RelayCommand(execute => 
            { 
                CurrentChildView = profilePanelUserControlVM;
                CurrentSelectedMenuPanelItem = MenuPanelItemEnum.ProfilePanel;
            }, 
            canExecute => 
            {
                return ServerConnectStatus == ServerConnectStatusEnum.Connected.ToString();
            });

            refreshSessionMainCommand = new RefreshSessionCommand(this);
            openRefreshSessionPanelCommand = new RelayCommand(execute =>
            {
                // stop refresh session timer
                CheckSessionOnExpireTimer?.Dispose();

                // close all others popUps
                // ...

                IsRefreshSessionPopupPanelOpen = true;
            },
            canExecute =>
            {
                return TokenRefreshStatus == TokenRefreshStatusEnum.TokenIsInvalid;
            });
            closeRefreshSessionPanelCommand = new RelayCommand(async execute => 
            {
                IsRefreshSessionPopupPanelOpen = false;
                await EmergencyShutdownLogout();
            },
            canExecute =>
            {
                return IsRefreshSessionPopupPanelOpen;
            });
            #endregion

            #region init event handlers
            botsPanelUserControlVM.ErrorHasOccurred += BotsPanelUserControlVM_ErrorHasOccurred;

            profilePanelUserControlVM.logoutCommand.SessionSuccessfullyDeleted += ProfilePanel_LogoutCommand_SessionSuccessfullyDeleted;
            profilePanelUserControlVM.logoutCommand.DeleteSessionFailed += ProfilePanel_LogoutCommand_DeleteSessionFailed;
            profilePanelUserControlVM.logoutCommand.ConnectionError += ProfilePanel_LogoutCommand_ConnectionError;

            connectErrorUserControlViewModel.ReconnectToServerCommand.SuccessfullyReconnected += ReconnectToServerCommand_SuccessfullyReconnected;
            connectErrorUserControlViewModel.ReconnectToServerCommand.ReconnectFailed += ReconnectToServerCommand_ReconnectFailed;

            refreshSessionMainCommand.SessionSuccessfullyRefreshed += RefreshSessionMainCommand_SessionSuccessfullyRefreshed;
            refreshSessionMainCommand.RefreshSessionFailed += RefreshSessionMainCommand_RefreshSessionFailed;
            refreshSessionMainCommand.ConnectionError += RefreshSessionMainCommand_ConnectionError;
            #endregion       

            // init session timer
            CheckSessionOnExpireTimer = new Timer(new TimerCallback(CheckSessionOnExpire), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));  
        }

        private void ReconnectToServerCommand_SuccessfullyReconnected()
        {
            ServerConnectStatus = ServerConnectStatusEnum.Undefined.ToString();
            ServerConnectStatus = ServerConnectStatusEnum.Connected.ToString();

            try
            {
                switch (CurrentSelectedMenuPanelItem)
                {
                    case MenuPanelItemEnum.AnalyticsPanel:
                        openAnalyticsPanelCommand.Execute(null);
                        break;
                    case MenuPanelItemEnum.TasksPanel:
                        openTasksPanelCommand.Execute(null);
                        break;
                    case MenuPanelItemEnum.BotsPanel:
                        openBotsPanelCommand.Execute(null);
                        break;
                    case MenuPanelItemEnum.SettingsPanel:
                        openSettingsPanelCommand.Execute(null);
                        break;
                    case MenuPanelItemEnum.ProfilePanel:
                        openProfilePanelCommand.Execute(null);
                        break;
                }
            }
            catch { }
        }
        private void ReconnectToServerCommand_ReconnectFailed()
        {
            ServerConnectStatus = ServerConnectStatusEnum.Undefined.ToString();
            ServerConnectStatus = ServerConnectStatusEnum.NotConnected.ToString();
        }

        private void BotsPanelUserControlVM_ErrorHasOccurred(string? error)
        {
            ServerConnectStatus = ServerConnectStatusEnum.Undefined.ToString();
            ServerConnectStatus = ServerConnectStatusEnum.NotConnected.ToString();

            CurrentChildView = connectErrorUserControlViewModel;
        }

        private void ProfilePanel_LogoutCommand_SessionSuccessfullyDeleted()
        {
            IsViewVisible = false;
        }
        private void ProfilePanel_LogoutCommand_DeleteSessionFailed(string obj)
        {
            ServerConnectStatus = ServerConnectStatusEnum.Undefined.ToString();
            ServerConnectStatus = ServerConnectStatusEnum.NotConnected.ToString();
            CurrentChildView = connectErrorUserControlViewModel;
        }
        private void ProfilePanel_LogoutCommand_ConnectionError()
        {
            ServerConnectStatus = ServerConnectStatusEnum.Undefined.ToString();
            ServerConnectStatus = ServerConnectStatusEnum.NotConnected.ToString();
            CurrentChildView = connectErrorUserControlViewModel;
        }

        private void RefreshSessionMainCommand_SessionSuccessfullyRefreshed()
        {
            // re-init session timer
            CheckSessionOnExpireTimer = new Timer(new TimerCallback(CheckSessionOnExpire), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            // re-write user canExecute actions
            botsPanelUserControlVM.ConfigureUserAction();
            //...and for all other panels

            IsRefreshSessionPopupPanelOpen = false;
            TokenRefreshStatus = TokenRefreshStatusEnum.TokenIsValid;
        }
        private async void RefreshSessionMainCommand_RefreshSessionFailed(string obj)
        {
            TokenRefreshStatus = TokenRefreshStatusEnum.TokenRefreshFail;

            if (closeRefreshSessionPanelCommand.CanExecute(null))
            {
                closeRefreshSessionPanelCommand.Execute(null);
            }
            else
            {
                await EmergencyShutdownLogout();
            }
        }
        private async void RefreshSessionMainCommand_ConnectionError()
        {
            TokenRefreshStatus = TokenRefreshStatusEnum.TokenRefreshFail;

            if (closeRefreshSessionPanelCommand.CanExecute(null))
            {
                closeRefreshSessionPanelCommand.Execute(null);
            }
            else
            {
                await EmergencyShutdownLogout();
            }
        }

        private async void CheckSessionOnExpire(object? parameter)
        {
            if (ServiceStorage.UserService.SessionIsExpired())
            {
                TokenRefreshStatus = TokenRefreshStatusEnum.TokenIsInvalid;

                if (openRefreshSessionPanelCommand.CanExecute(null))
                {
                    openRefreshSessionPanelCommand.Execute(null);
                }
                else
                {
                    await EmergencyShutdownLogout();
                }
            }
        }
        private async Task EmergencyShutdownLogout()
        {
            // logout without server notify
            try
            {
                // delete refresh session timer
                CheckSessionOnExpireTimer?.Dispose();

                // delete access token
                ServiceStorage.UserService.SetAccessToken("");

                // delete refresh token
                ServiceStorage.UserService.SetRefreshToken("");

                // disconnect from server hub
                CancellationTokenSource cancellationTokenSourceDisconnect = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await ServiceStorage.ServerConnectService.DisconnectFromServerHub(cancellationTokenSourceDisconnect.Token);

                // hide view
                IsViewVisible = false;
            }
            catch { }
        }
    }
}
