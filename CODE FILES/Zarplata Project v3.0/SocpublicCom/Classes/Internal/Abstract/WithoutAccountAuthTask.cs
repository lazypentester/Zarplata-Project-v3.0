using CommonModels.Client;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocpublicCom.Classes.Internal.Abstract
{
    internal abstract class WithoutAccountAuthTask : EarningSiteSocpublicComTask
    {
        private protected WithoutAccountAuthTask(EarningSiteWorkBotClient client, SocpublicComTask task, HubConnection serverHubConnection, HttpClient serverHttpConnection) : base(client, serverHubConnection, serverHttpConnection, task: task)
        {
        }
    }
}
