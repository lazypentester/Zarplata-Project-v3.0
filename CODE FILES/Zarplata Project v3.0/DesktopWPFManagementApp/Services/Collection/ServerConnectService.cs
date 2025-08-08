using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopWPFManagementApp.Services.Collection
{
    public class ServerConnectService
    {
        //static string authenticationString = "11186570:60-dayfreetrial";
        //static string base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

        public HttpClient SERVER_HTTP_CONNECTION { get; set; }
        public HubConnection SERVER_HUB_CONNECTION { get; set; }

        public ServerConnectService()
        {
            #region InitConnections
            SERVER_HTTP_CONNECTION = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(5),
                BaseAddress = new Uri($"{AppConfig.SERVER_HOST}/api/")
            };

            SERVER_HUB_CONNECTION = new HubConnectionBuilder()
                    .WithUrl($"{AppConfig.SERVER_HOST}/hubs/management", options =>
                    {
                        options.AccessTokenProvider = () =>
                        {
                            return Task.FromResult(ServiceStorage.UserService.GetAccessToken());
                        };

                        options.Headers.Add("access-control-allow-origin", "*");
                        options.SkipNegotiation = true;
                        options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;

                        // remove after
                        //options.Headers.Add("Authorization", $"Basic {base64EncodedAuthenticationString}");
                    })
                    .WithAutomaticReconnect()
                    .WithServerTimeout(TimeSpan.FromSeconds(60))
                    .Build();
            #endregion
        }

        public async Task TestServerConnection(CancellationToken cancellationToken)
        {
            #region Подключение к серверу

            Uri? endpoint = null;
            HttpResponseMessage? responce = null;
            string? content = null;

            endpoint = new Uri("test/connect", UriKind.Relative);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = endpoint
            };

            //request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            responce = await SERVER_HTTP_CONNECTION.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

            if (!responce.IsSuccessStatusCode)
            {
                content = await responce.Content.ReadAsStringAsync();
                throw new Exception("Status code: " + responce.StatusCode);
            }

            endpoint = null;
            responce = null;
            content = null;

            #endregion
        }
        public async Task TestServerHubConnection(CancellationToken cancellationToken)
        {
            try
            {
                await SERVER_HUB_CONNECTION.StartAsync(cancellationToken);
            }
            catch (Exception e)
            {
            }
        }
        public async Task ReconnectToServerHub(CancellationToken cancellationTokenDisconnect, CancellationToken cancellationTokenConnect)
        {
            if(SERVER_HUB_CONNECTION.State == HubConnectionState.Reconnecting ||
                SERVER_HUB_CONNECTION.State == HubConnectionState.Connecting ||
                SERVER_HUB_CONNECTION.State == HubConnectionState.Connected)
            {
                await SERVER_HUB_CONNECTION.StopAsync(cancellationTokenDisconnect);
            }

            if(SERVER_HUB_CONNECTION.State == HubConnectionState.Disconnected)
            {
                await SERVER_HUB_CONNECTION.StartAsync(cancellationTokenConnect);
            }
        }
        public async Task DisconnectFromServerHub(CancellationToken cancellationTokenDisconnect)
        {
            if (SERVER_HUB_CONNECTION.State == HubConnectionState.Reconnecting ||
                SERVER_HUB_CONNECTION.State == HubConnectionState.Connecting ||
                SERVER_HUB_CONNECTION.State == HubConnectionState.Connected)
            {
                await SERVER_HUB_CONNECTION.StopAsync(cancellationTokenDisconnect);
            }
        }
    }
}
