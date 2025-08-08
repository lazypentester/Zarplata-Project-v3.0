using Microsoft.AspNetCore.SignalR;
using Server.Hubs;

namespace Server.Database.Services
{
    public class ProxyTasksManagementService
    {
        private readonly ProxyTasksService proxyTasksService;
        private readonly IHubContext<ManagementHub> ManagementHubContext;

        public ProxyTasksManagementService(ProxyTasksService proxyTasksService, IHubContext<ManagementHub> managementHubContext)
        {
            this.proxyTasksService = proxyTasksService;
            ManagementHubContext = managementHubContext;
        }

        public void AddToQueue()
        {

        }
    }
}
