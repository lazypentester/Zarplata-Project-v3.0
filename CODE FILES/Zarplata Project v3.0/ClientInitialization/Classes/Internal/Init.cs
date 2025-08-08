using CommonModels;
using CommonModels.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static ClientInitialization.Classes.Public.InitDelegates;
using static ClientOperationMessages.InitMessages;
using static CommonModels.Client.Client;
using static CommonModels.Client.Machine;

namespace ClientInitialization.Classes.Internal
{
    internal abstract class Init : IDisposable
    {
        //static string authenticationString = "11186570:60-dayfreetrial";
        //static string base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

        HubConnection? serverHubConnection = null;
        HttpClient serverhttpConnection;

        PrintMessage printMessageMethods;

        Client client;

        string[] appParams;

        private bool disposedValue;

        private protected Init(IEnumerable<string> appParams, Client client, PrintMessage printMessageMethods, Platform OsPlatform, string OSPlatformText)
        {
            this.appParams = appParams.ToArray();
            this.client = client;
            client.MACHINE.OS_PLATFORM_TEXT = OSPlatformText;
            client.MACHINE.OS_PLATFORM = OsPlatform;

            serverhttpConnection = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(60)
            };

            this.printMessageMethods = printMessageMethods;
        }

        internal async Task StartInit()
        {
            string TOKEN = "";

            // получение значения --server-host / -sh
            printMessageMethods.Invoke($"\n{OPERATION_START}Получение адреса сервера");
            var GetServerHostResult = GetServerHost();
            if (!GetServerHostResult.Key || GetServerHostResult.Value == null)
            {
                printMessageMethods.Invoke($"{OPERATION_FAILED}");
                throw new Exception("Ошибка при получении адреса хоста сервера.\n" + GetServerHostResult.Value);
            }
            client.SERVER_HOST = GetServerHostResult.Value;
            printMessageMethods.Invoke($"{OPERATION_SUCCESSFUL}");

            // Создание идентификационного ключа устрйоства
            printMessageMethods.Invoke($"\n{OPERATION_START}Создание идентификационного ключа устройства");
            var CreateIdentityKeyResult = CreateIdentityKey();
            if (!CreateIdentityKeyResult.Key)
            {
                printMessageMethods.Invoke($"{OPERATION_FAILED}");
                throw new Exception("Ошибка при создании идентификационного ключа.\n" + CreateIdentityKeyResult.Value);
            }
            client.MACHINE.IDENTITY_KEY = CreateIdentityKeyResult.Value;
            printMessageMethods.Invoke($"{OPERATION_SUCCESSFUL}");

            // Сохраняем некоторые параметры устройства в обьект Client (без вывода сооб.)
            var SetDeviceDataResult = SetDeviceData();
            if (!SetDeviceDataResult.Key)
            {
                throw new Exception("Ошибка в методе 'SetDeviceData'.\n" + SetDeviceDataResult.Value);
            }

            // Проверка соединения с сервером
            printMessageMethods.Invoke($"\n{OPERATION_START}Проверка соединения с сервером");
            Task TestServerConnResult = Task.Run(TestServerConnection);
            try
            {
                await TestServerConnResult;
            }
            catch (Exception e)
            {
                printMessageMethods.Invoke($"{OPERATION_FAILED}");
                throw new Exception($"Ошибка при подключении к серверу..\n" + e.Message);
            }
            TestServerConnResult.Dispose();
            printMessageMethods.Invoke($"{OPERATION_SUCCESSFUL}");

            // Инициализация обьекта хаба (без вывода сооб.)
            var CreateHubConnectionResult = CreateHubConnection();
            if (!CreateHubConnectionResult.Key)
            {
                throw new Exception("error" + CreateHubConnectionResult.Value);
            }

            // Проверка соединения с хабом на сервере
            printMessageMethods.Invoke($"\n{OPERATION_START}Проверка соединения с хабом на сервере");
            Task TestHubConnResult = Task.Run(TestServerHubConnection);
            try
            {
                await TestHubConnResult;
            }
            catch (Exception e)
            {
                printMessageMethods.Invoke($"{OPERATION_FAILED}");
                throw new Exception("Ошибка при подключении к хабу на сервере\n" + e.Message);
            }
            TestHubConnResult.Dispose();
            printMessageMethods.Invoke($"{OPERATION_SUCCESSFUL}");

            // Создание сессии клиента
            printMessageMethods.Invoke($"\n{OPERATION_START}Создание сессии клиента");
            Task<string> CreateClientResult = Task.Run(CreateClientSession);
            try
            {
                TOKEN = await CreateClientResult;
            }
            catch (Exception e)
            {
                printMessageMethods.Invoke($"{OPERATION_FAILED}");
                throw new Exception($"Ошибка при создании клиента\n" + e.Message);
            }
            CreateClientResult.Dispose();
            printMessageMethods.Invoke($"{OPERATION_SUCCESSFUL}");

            // Запись токена сессии (без вывода сооб.)
            try
            {
                client.WriteEnvironmentVariable(nameof(TOKEN), TOKEN, EnvironmentVariableTarget.Process);
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка при записи токена сессии\n" + e.Message);
            }

            // Переподключение к хабу с токеном доступа
            printMessageMethods.Invoke($"\n{OPERATION_START}Переподключение к хабу с токеном доступа");
            Task ReconnectToHubWithTokenResult = Task.Run(ReconnectToServerHubWithToken);
            try
            {
                await ReconnectToHubWithTokenResult;
            }
            catch (Exception e)
            {
                printMessageMethods.Invoke($"{OPERATION_FAILED}");
                throw new Exception("\nОшибка при переподключении к хабу с токеном\n" + e.Message);
            }
            ReconnectToHubWithTokenResult.Dispose();
            printMessageMethods.Invoke($"{OPERATION_SUCCESSFUL}");

            // изменение статуса бота с "подключен" на "свободен" (без вывода сооб.)
            Task changeStatusToFromConnectedToFreeResult = Task.Run(() => ChangeStatusToFromConnectedToFree());
            try
            {
                await changeStatusToFromConnectedToFreeResult;
            }
            catch (Exception e)
            {
                throw new Exception($"\nОшибка при изменении статуса бота\n" + e.Message);
            }
            changeStatusToFromConnectedToFreeResult.Dispose();

            // Запрос на получение по необходимости нового задания для текущего подключившигося бота (без вывода сооб.)
            Task сheckOnNeedToGiveTaskToConnectedBot = Task.Run(() => CheckOnNeedToGiveTaskToConnectedBot(client.Role!.Value));
            try
            {
                await сheckOnNeedToGiveTaskToConnectedBot;
            }
            catch (Exception e)
            {
                throw new Exception($"\nОшибка при запроск на получение по необходимости нового задания для текущего подключившигося бота\n" + e.Message);
            }
            сheckOnNeedToGiveTaskToConnectedBot.Dispose();

            // Добавление клиента в групу хаба (без вывода сооб.)
            Task AddClientToGroupResult = Task.Run(() => AddClientToHubGroup(client.Role.ToString() + ""));
            try
            {
                await AddClientToGroupResult;
            }
            catch (Exception e)
            {
                throw new Exception($"\nОшибка при добавлении клиента в группу\n" + e.Message);
            }
            AddClientToGroupResult.Dispose();
        }

        public HubConnection? get_SERVER_HUB_CONNECTION() => serverHubConnection;
        public HttpClient get_SERVER_HTTP_CONNECTION() => serverhttpConnection;
        private KeyValuePair<bool, string?> GetServerHost()
        {
            // проверка параметров на наличие --server-host / -sh

            string? host = null;

            for (int i = 0; i < appParams.Length; i++)
            {
                if (appParams[i] == "--server-host" || appParams[i] == "-sh")
                {
                    if (i == appParams.Length - 1)
                    {
                        return new KeyValuePair<bool, string?>(false, "Not found 'server host variable value'");
                    }

                    host = appParams[i + 1];

                    if (host != null)
                    {
                        return new KeyValuePair<bool, string?>(true, host);
                    }
                    else
                    {
                        return new KeyValuePair<bool, string?>(false, "'server host variable' must be not null value");
                    }
                }
            }

            return new KeyValuePair<bool, string?>(false, "Not set 'server host variable'");
        }
        internal abstract List<KeyValuePair<string, string?>> CollectIdentityKeyData();
        internal virtual KeyValuePair<bool, string> CreateIdentityKey()
        {
            string IDENTITY_KEY = "";

            var data = CollectIdentityKeyData();

            // Убрать проверку на null?
            foreach(var part in data)
            {
                if (part.Value == null)
                    return new KeyValuePair<bool, string>(false, $"Collected identity key data '{part.Key}' is null");
            }

            using (var md5 = MD5.Create())
            {
                StringBuilder? builder = null;
                byte[]? hashInBytes = null;
                string? hashInString = null;

                try
                {
                    builder = new StringBuilder();

                    foreach (KeyValuePair<string, string?> part in data)
                    {
                        builder.Append(part.Value);
                    }

                    hashInBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(builder.ToString()));
                    hashInString = Convert.ToHexString(hashInBytes);

                    IDENTITY_KEY = hashInString;
                }
                catch(Exception ex)
                {
                    return new KeyValuePair<bool, string>(false, ex.Message);
                }
                finally
                {
                    data = null;
                    builder = null;
                    hashInBytes = null;
                    hashInString = null;
                }
            }

            return new KeyValuePair<bool, string>(true, IDENTITY_KEY);
        }
        private KeyValuePair<bool, string> SetDeviceData()
        {
            // set machineName && userName to machine obj class
            try
            {
                client.MACHINE.MACHINE_NAME = System.Environment.MachineName;
                client.MACHINE.USER_NAME = System.Environment.UserName;
            }
            catch(Exception ex)
            {
                return new KeyValuePair<bool, string>(false, $"{ex.Message}");
            }

            return new KeyValuePair<bool, string>(true, $"Set machine data is successfull");
        }
        private async Task TestServerConnection()
        {
            #region Подключение к серверу

            Uri? endpoint = null;
            HttpResponseMessage? responce = null;
            string? content = null;

            endpoint = new Uri($"{client.SERVER_HOST}/api/test/connect");

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = endpoint
            };

            //request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            responce = await serverhttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);

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
        private KeyValuePair<bool, string> CreateHubConnection()
        {
            try
            {
                serverHubConnection = new HubConnectionBuilder()
                .WithUrl($"{client.SERVER_HOST}/hubs/client", options =>
                {
                    options.AccessTokenProvider = () =>
                    {
                        return Task.FromResult(client.ReadEnvironmentVariable("TOKEN", EnvironmentVariableTarget.Process));
                    };

                    options.Headers.Add("access-control-allow-origin", "*");
                    options.SkipNegotiation = true;
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                    //options.CloseTimeout.Add(TimeSpan.FromSeconds(10));

                    // remove after
                    //options.Headers.Add("Authorization", $"Basic {base64EncodedAuthenticationString}");
                })
                .WithAutomaticReconnect()
                .WithServerTimeout(TimeSpan.FromSeconds(60))
                .Build();
            }
            catch (Exception e)
            {
                return new KeyValuePair<bool, string>(false, $"Ошибка при создании подключения к хабу\n{e.Message}");
            }

            return new KeyValuePair<bool, string>(true, "Create Hub Connection is successfull");
        }
        private async Task TestServerHubConnection()
        {
            await serverHubConnection!.StartAsync();
        }
        private async Task<string> CreateClientSession()
        {
            Uri? endpoint = null;
            JsonContent? jsonContent = null;
            HttpResponseMessage? responce = null;
            string? content = null;
            Dictionary<string, string>? resultFromServer = null;

            endpoint = new Uri($"{client.SERVER_HOST}/api/client/session/create");
            jsonContent = JsonContent.Create(client);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = endpoint,
                Content = jsonContent
            };

            //request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            //remove after
            //request.Headers.Add("Remote_Addr", "5.199.232.23");
            //remove after

            responce = await serverhttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            content = await responce.Content.ReadAsStringAsync();

            if (!responce.IsSuccessStatusCode)
            {
                throw new Exception($"Не удалось создать клиент\n{content}");
            }

            resultFromServer = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            client.ID = resultFromServer!.Where(item => item.Key == "id").FirstOrDefault().Value;
            client.IP = resultFromServer!.Where(item => item.Key == "ip").FirstOrDefault().Value;
            string token = resultFromServer!.Where(item => item.Key == "token").FirstOrDefault().Value;  

            if (token == null)
            {
                throw new Exception($"Ошибка при получении токена сессии\ntoken == null");
            }

            endpoint = null;
            jsonContent = null;
            responce = null;
            content = null;
            resultFromServer = null;

            return token;
        }
        private async Task ReconnectToServerHubWithToken()
        {
            await serverHubConnection!.StopAsync();
            //await Task.Delay(TimeSpan.FromSeconds(5));
            await serverHubConnection!.StartAsync();

            if (serverHubConnection.State == HubConnectionState.Disconnected)
            {
                throw new Exception("suka");
            }
        }
        private async Task ChangeStatusToFromConnectedToFree()
        {
            //await Task.Delay(TimeSpan.FromSeconds(20));
            if (serverHubConnection!.State == HubConnectionState.Disconnected)
            {
                throw new Exception("suka");
            }

            await serverHubConnection!.SendAsync("change_bot_status_from_connected_to_free");

            if (serverHubConnection.State == HubConnectionState.Disconnected)
            {
                throw new Exception("suka");
            }
        }
        private async Task CheckOnNeedToGiveTaskToConnectedBot(BotRole role)
        {
            await serverHubConnection!.SendAsync("check_on_need_to_give_task_to_connected_bot", role);
        }
        private async Task AddClientToHubGroup(string groupName)
        {
            //await serverHubConnection!.SendAsync("addClientToHubGroup", groupName);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)

                    if (serverhttpConnection != null)
                    {
                        //httpClient.Dispose();
                    }

                    serverHubConnection = null;
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей

                //if (serverHubConnection != null) serverHubConnection = null;

                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~Init()
        // {
        //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
