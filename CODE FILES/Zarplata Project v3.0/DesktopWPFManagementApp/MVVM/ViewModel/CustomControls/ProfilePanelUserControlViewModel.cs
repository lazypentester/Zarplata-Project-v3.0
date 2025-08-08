using DesktopWPFManagementApp.Commands;
using DesktopWPFManagementApp.MVVM.ViewModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopWPFManagementApp.MVVM.ViewModel.CustomControls
{
    internal class ProfilePanelUserControlViewModel : ViewModelBase
    {
        #region Commands
        public LogoutCommand logoutCommand { get; }
        #endregion

        public ProfilePanelUserControlViewModel()
        {
            #region Init Commands
            logoutCommand = new LogoutCommand();
            #endregion
        }
    }
}
