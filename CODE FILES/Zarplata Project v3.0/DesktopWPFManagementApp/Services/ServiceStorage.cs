using DesktopWPFManagementApp.Services.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopWPFManagementApp.Services
{
    public static class ServiceStorage
    {
        public static SystemService SystemService { get; private set; } = new SystemService();
        public static ServerConnectService ServerConnectService { get; private set; } = new ServerConnectService();
        public static UserService UserService { get; private set; } = new UserService(ServerConnectService.SERVER_HTTP_CONNECTION, ServerConnectService.SERVER_HUB_CONNECTION, SystemService);
        public static BotService BotService { get; private set; } = new BotService(ServerConnectService.SERVER_HUB_CONNECTION);
        public static EarnTaskService EarnTaskService { get; private set; } = new EarnTaskService(ServerConnectService.SERVER_HUB_CONNECTION);
    }
}
