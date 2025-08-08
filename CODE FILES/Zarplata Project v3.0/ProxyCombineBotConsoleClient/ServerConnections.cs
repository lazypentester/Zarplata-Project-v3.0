using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyCombineBotConsoleClient
{
    internal static class ServerConnections
    {
        internal static HubConnection? serverHubConnection = null;
        internal static HttpClient? serverhttpConnection = null;
    }
}
