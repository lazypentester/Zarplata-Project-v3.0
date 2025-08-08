using DesktopWPFManagementApp.Commands;
using DesktopWPFManagementApp.MVVM.ViewModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static DesktopWPFManagementApp.MVVM.ViewModel.CustomControls.AuthPanelUserControlViewModel;

namespace DesktopWPFManagementApp.MVVM.ViewModel.CustomControls
{
    internal class ConnectErrorUserControlViewModel : ViewModelBase
    {
        #region Enums
        public enum ReconnectStatusEnum
        {
            ReadyToReconnect,
            IsReconnect
        }
        #endregion

        private string reconnectStatus = ReconnectStatusEnum.ReadyToReconnect.ToString();

        public string ReconnectStatus
        {
            get { return reconnectStatus; }
            set
            {
                reconnectStatus = value;
                OnPropertyChanged();
            }
        }

        public ReconnectToServerCommand ReconnectToServerCommand { get; }

        public ConnectErrorUserControlViewModel()
        {
            ReconnectToServerCommand = new ReconnectToServerCommand(this);
        }
    }
}
