using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.User.Session
{
    public class LogoutedModel
    {
        public string LogoutStatusText { get; set; }
        public bool LogoutStatus { get; set; }

        public LogoutedModel(string logoutStatusText, bool logoutStatus)
        {
            LogoutStatusText = logoutStatusText;
            LogoutStatus = logoutStatus;
        }
    }
}
