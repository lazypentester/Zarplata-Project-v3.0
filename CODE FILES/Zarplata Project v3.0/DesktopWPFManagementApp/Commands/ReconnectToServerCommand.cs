using DesktopWPFManagementApp.MVVM.ViewModel.CustomControls;
using DesktopWPFManagementApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.AuthPanelUserControlViewModel;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.ConnectErrorUserControlViewModel;

namespace DesktopWPFManagementApp.Commands
{
    internal class ReconnectToServerCommand : RelayCommand
    {
        private ConnectErrorUserControlViewModel connectErrorUserControlVM;

        //events
        public event Action SuccessfullyReconnected = () => { };
        public event Action ReconnectFailed = () => { };

        public ReconnectToServerCommand(ConnectErrorUserControlViewModel connectErrorUserControlVM)
        {
            this.connectErrorUserControlVM = connectErrorUserControlVM;
        }

        public override bool CanExecute(object? parameter)
        {
            return connectErrorUserControlVM.ReconnectStatus == ReconnectStatusEnum.ReadyToReconnect.ToString() && base.CanExecute(parameter);
        }

        public override async void Execute(object? parameter)
        {
            CancellationTokenSource cancellationTokenSourceReConnectToServer = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            CancellationTokenSource cancellationTokenSourceReConnectDisconnectToServerHub = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            CancellationTokenSource cancellationTokenSourceReConnectConnectToServerHub = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            try
            {
                connectErrorUserControlVM.ReconnectStatus = ReconnectStatusEnum.IsReconnect.ToString();

                await ServiceStorage.ServerConnectService.TestServerConnection(cancellationTokenSourceReConnectToServer.Token);
                await ServiceStorage.ServerConnectService.ReconnectToServerHub(cancellationTokenSourceReConnectDisconnectToServerHub.Token, cancellationTokenSourceReConnectConnectToServerHub.Token);

                SuccessfullyReconnected?.Invoke();
            }
            catch (Exception e)
            {
                ReconnectFailed?.Invoke();
            }
            finally
            {
                connectErrorUserControlVM.ReconnectStatus = ReconnectStatusEnum.ReadyToReconnect.ToString();
            }
        }
    }
}
