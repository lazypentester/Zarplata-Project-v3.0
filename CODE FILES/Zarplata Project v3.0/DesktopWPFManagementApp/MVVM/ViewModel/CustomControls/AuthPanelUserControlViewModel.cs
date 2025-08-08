using DesktopWPFManagementApp.Commands;
using DesktopWPFManagementApp.MVVM.ViewModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DesktopWPFManagementApp.MVVM.ViewModel.CustomControls
{
    internal class AuthPanelUserControlViewModel : ViewModelBase
    {
        #region Enums
        public enum AuthStatusEnum
        {
            NotReadyToAuth,
            ReadyToAuth,
            IsAuth,
            SuccessAuth,
            FailAuth
        }
        #endregion

        #region Properties
        private string authStatus = AuthStatusEnum.NotReadyToAuth.ToString();
        public string AuthStatus
        {
            get { return authStatus; }
            set
            {
                authStatus = value;
                OnPropertyChanged();
            }
        }

        private string authFailStatusText = "Неверный логин или пароль";
        public string AuthFailStatusText
        {
            get { return authFailStatusText; }
            set
            {
                authFailStatusText = value;
                OnPropertyChanged();
            }
        }

        private string username = "";
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                updateAuthStatusProperty();
                updateAuthCredential(username: value);
                OnPropertyChanged();
            }
        }

        private string password = "";
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                updateAuthStatusProperty();
                updateAuthCredential(password: value);
                OnPropertyChanged();
            }
        }

        private NetworkCredential authCredential = new NetworkCredential();
        public NetworkCredential AuthCredential
        {
            get { return authCredential; }
            set { authCredential = value; }
        }
        #endregion

        #region Commands
        //public ICommand AuthCommand { get; }
        public AuthCommand AuthCommand { get; }
        #endregion

        public AuthPanelUserControlViewModel()
        {
            // init commands
            AuthCommand = new AuthCommand(this);
        }

        private void updateAuthStatusProperty()
        {
            if(Username.Length >= 3 && Password.Length >= 3)
            {
                AuthStatus = AuthStatusEnum.ReadyToAuth.ToString();
            }
            else
            {
                AuthStatus = AuthStatusEnum.NotReadyToAuth.ToString();
            }
        }

        private void updateAuthCredential(string? username = null, string? password = null)
        {
            if(username != null)
                authCredential.UserName = username;
            if(password != null)
                authCredential.Password = password;
        }
    }
}
