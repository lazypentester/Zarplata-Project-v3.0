using CommonModels.Client;
using CommonModels.ProjectTask.ProxyCombiner;
using CommonModels.ProjectTask.ProxyCombiner.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using ProxyCombiner.Classes.Public;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static ClientInitialization.Classes.Public.InitDelegates;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using static CommonModels.ProjectTask.ProxyCombiner.Models.EnvironmentProxy;
using static ClientOperationMessages.InitMessages;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.DefaultCombineTask;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.SpecialCombineTask;
using ProxyCombiner.Classes.Internal;

namespace ProxyCombineBotConsoleClient
{
    internal class Program
    {
        static Client client = new ProxyCombineBotClient();
        static PrintMessage printMessageMethods = new Init().PrintClientMessage;
        static bool CLIENT_IS_BUSY_NOW = false;

        static HttpClient? ProxyParseHttpClient = null;
        static CookieContainer? ProxyParseCookieContainer = null;

        static DoProxyCombinerTask doProxyCombinerTask = new DoProxyCombinerTask();
        static SelectiveProxyCombinerTask? proxyCombinerTask = null;

        static bool BOT_IS_DISPOSING = false;

        static async Task Main(string[] args)
        {
            if (client.ReferenceCall(args))
            {
                Environment.Exit(0);
            }

            // Процесс инициализации клиента
            await new Init().Initialize(args, client, printMessageMethods);

            // регистрация методов хаба
            RegisterTaskHubMethods();

            // Процесс инициализации http клиента для парсинга прокси
            await GetEnvironmentProxyAndInitParseHttpClient();

            Thread.Sleep(Timeout.Infinite); // еще раз почитать правильно ли так делать
        }

        private static void RegisterTaskHubMethods()
        {
            #region Исполняемые методы SignalR

            ServerConnections.serverHubConnection!.On<CheckTask>("do_new_proxycombiner_check_task", async (task) =>
            {
                //removeafterthis!!!--------
                Console.WriteLine($"Получили новое задание (проверка проксей).");

                // проверка клиента на занятость
                /*if (CLIENT_IS_BUSY_NOW)
                {
                    await clientIsBusy(task.Id!);
                    return;
                }*/

                // wait for disposing
                try
                {
                    await waitForDisposing();
                }
                catch(Exception e)
                {
                    //removeafterthis!!!--------
                    await SaveMessage.Save(client!.ID!, $"waitForDisposing, error {e.Message}");
                    Console.WriteLine($"waitForDisposing, error {e.Message}");
                }

                // подтверждение принятия задания
                /*try
                {
                    await ServerConnections.serverHubConnection!.InvokeAsync("task_receipt_confirmation", task.Id);
                    CLIENT_IS_BUSY_NOW = true;
                }
                catch { }*/

                // change bot status to AtWork
                //try
                //{
                //    await ServerConnections.serverHubConnection!.SendAsync("set_bot_status_to_atwork");
                //}
                //catch { }

                // запуск задания
                try
                {
                    await doProxyCombinerTask.Do(task, ServerConnections.serverHubConnection!, ProxyParseHttpClient!, ProxyParseCookieContainer!, client);

                    try
                    {
                        await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Success);
                    }
                    catch(Exception e)
                    {
                        //removeafterthis!!!--------
                        await SaveMessage.Save(client!.ID!, $"await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Success);, error {e.Message}");
                        Console.WriteLine($"await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Success);, error {e.Message}");
                    }
                }
                catch (Exception e)
                {
                    //removeafterthis!!!--------
                    await SaveMessage.Save(client!.ID!, $", error {e.Message}");
                    Console.WriteLine($", error {e.Message}");

                    try
                    {
                        await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Error, e);
                    }
                    catch
                    {
                        //removeafterthis!!!--------
                        await SaveMessage.Save(client!.ID!, $"await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Error, e);, error {e.Message}");
                        Console.WriteLine($"await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Error, e);, error {e.Message}");
                    }
                }
            });
            ServerConnections.serverHubConnection!.On<ParseTask>("do_new_proxycombiner_parse_task", async (task) =>
            {
                // проверка клиента на занятость
                /*if (CLIENT_IS_BUSY_NOW)
                {
                    await clientIsBusy(task.Id!);
                    return;
                }*/

                // wait for disposing
                try
                {
                    await waitForDisposing();
                }
                catch { }

                // подтверждение принятия задания
                /*try
                {
                    await ServerConnections.serverHubConnection!.InvokeAsync("task_receipt_confirmation", task.Id);
                    CLIENT_IS_BUSY_NOW = true;
                }
                catch { }*/

                // change bot status to AtWork
                try
                {
                    await ServerConnections.serverHubConnection!.SendAsync("set_bot_status_to_atwork");
                }
                catch { }

                // запуск задания
                try
                {
                    await doProxyCombinerTask.Do(task, ServerConnections.serverHubConnection!, ProxyParseHttpClient!, ProxyParseCookieContainer!, client);

                    try
                    {
                        await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Success);
                    }
                    catch { }
                }
                catch (Exception e)
                {
                    try
                    {
                        await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Error, e);
                    }
                    catch { }
                }
            });
            ServerConnections.serverHubConnection!.On<DefaultCombineTask>("do_new_proxycombiner_defaultcombine_task", async (task) =>
            {
                // проверка клиента на занятость
                /*if (CLIENT_IS_BUSY_NOW)
                {
                    await clientIsBusy(task.Id!);
                    return;
                }*/

                // wait for disposing
                try
                {
                    await waitForDisposing();
                }
                catch { }

                // change bot status to AtWork
                try
                {
                    await ServerConnections.serverHubConnection!.SendAsync("set_bot_status_to_atwork");
                }
                catch { }

                // подтверждение принятия задания
                /*try
                {
                    await ServerConnections.serverHubConnection!.InvokeAsync("task_receipt_confirmation", task.Id);
                    CLIENT_IS_BUSY_NOW = true;
                }
                catch { }*/

                // запуск задания
                try
                {
                    await doProxyCombinerTask.Do(task, ServerConnections.serverHubConnection!, ProxyParseHttpClient!, ProxyParseCookieContainer!, client);

                    try
                    {
                        await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Success);
                    }
                    catch { }
                }
                catch (Exception e)
                {
                    try
                    {
                        await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Error, e);
                    }
                    catch { }
                }
            });
            ServerConnections.serverHubConnection!.On<SpecialCombineTask>("do_new_proxycombiner_specialcombine_task", async (task) =>
            {
                //removeafterthis!!!--------
                Console.WriteLine($"Получили новое задание (specialcombine).");

                // проверка клиента на занятость
                /*if (CLIENT_IS_BUSY_NOW)
                {
                    await clientIsBusy(task.Id!);
                    return;
                }*/

                // wait for disposing
                try
                {
                    await waitForDisposing();
                }
                catch(Exception e)
                {
                    //removeafterthis!!!--------
                    await SaveMessage.Save(client!.ID!, $"await waitForDisposing();, error {e.Message}");
                    Console.WriteLine($"await waitForDisposing();, error {e.Message}");
                }

                // подтверждение принятия задания
                /*try
                {
                    await ServerConnections.serverHubConnection!.InvokeAsync("task_receipt_confirmation", task.Id);
                    CLIENT_IS_BUSY_NOW = true;
                }
                catch { }*/

                // change bot status to AtWork
                //try
                //{
                //    await ServerConnections.serverHubConnection!.SendAsync("set_bot_status_to_atwork");
                //}
                //catch { }

                // запуск задания
                try
                {
                    await doProxyCombinerTask.Do(task, ServerConnections.serverHubConnection!, ProxyParseHttpClient!, ProxyParseCookieContainer!, client);

                    try
                    {
                        await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Success);
                    }
                    catch(Exception e)
                    {
                        //removeafterthis!!!--------
                        await SaveMessage.Save(client!.ID!, $"await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Success);, error {e.Message}");
                        Console.WriteLine($"await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Success);, error {e.Message}");
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Error, e);
                    }
                    catch(Exception ex)
                    {
                        //removeafterthis!!!--------
                        await SaveMessage.Save(client!.ID!, $"await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Error, e);, error {ex.Message}");
                        Console.WriteLine($"await doProxyCombinerTask.TaskChangedStatus(TaskResultStatus.Error, e);, error {ex.Message}");
                    }
                }
            });

            ServerConnections.serverHubConnection!.On<string>("dispose_proxycombiner_and_set_new_token", async (token) =>
            {
                //if (CLIENT_IS_BUSY_NOW)
                //{
                //    await clientIsBusy();
                //    return;
                //}

                BOT_IS_DISPOSING = true;

                try
                {
                    doProxyCombinerTask.DisposeTask();
                    CLIENT_IS_BUSY_NOW = false;
                }
                catch (Exception e)
                {
                    //removeafterthis!!!--------
                    await SaveMessage.Save(client!.ID!, $"doProxyCombinerTask.DisposeTask();, error {e.Message}");
                    Console.WriteLine($"doProxyCombinerTask.DisposeTask();;, error {e.Message}");

                    try
                    {
                        await clientInternalError(e.Message);
                    }
                    catch(Exception ex)
                    {
                        //removeafterthis!!!--------
                        await SaveMessage.Save(client!.ID!, $"await clientInternalError(e.Message);;, error {ex.Message}");
                        Console.WriteLine($"await clientInternalError(e.Message);;, error {ex.Message}");
                    }
                }

                // get and set new token
                //try
                //{
                //    await clientRenewAccessToken();
                //}
                //catch (Exception)
                //{

                //}

                BOT_IS_DISPOSING = false;

            });

            ServerConnections.serverHubConnection!.On("bot_deleted", () =>
            {
                // clear memory and close bot
                Environment.Exit(0);
            });

            #endregion
        }
        private static async Task GetEnvironmentProxyAndInitParseHttpClient()
        {
            printMessageMethods.Invoke($"\n{OPERATION_START}Инициализация HttpClient для парсинга");

            EnvironmentProxy? environmentProxy = null;

            try
            {
                Uri? endpoint = null;
                HttpResponseMessage? responce = null;
                string? content = null;
                List<EnvironmentProxy>? resultFromServer = null;

                endpoint = new Uri($"{client.SERVER_HOST}/api/client/environmentproxy/{EPMarker.Parser}");

                ServerConnections.serverhttpConnection!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", client.ReadEnvironmentVariable("TOKEN", EnvironmentVariableTarget.Process));
                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = endpoint
                };

                //request.Headers.Authorization = new AuthenticationHeaderValue("Basic", "MTExODY1NzA6NjAtZGF5ZnJlZXRyaWFs");

                // remove after
                //request.Headers.Add("Remote_Addr", "5.199.232.23");
                // remove after

                responce = await ServerConnections.serverhttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                content = await responce.Content.ReadAsStringAsync();

                if (!responce.IsSuccessStatusCode)
                {
                    throw new Exception($"Не удалось получить прокси для работы парсера\n{content}");
                }

                resultFromServer = JsonConvert.DeserializeObject<List<EnvironmentProxy>>(content);

                if(resultFromServer != null && resultFromServer.Count > 0)
                {
                    switch (resultFromServer.Count)
                    {
                        case 1:
                            environmentProxy = resultFromServer[0];
                            break;
                        case > 1:
                            environmentProxy = resultFromServer[new Random().Next(0, resultFromServer.Count - 1)];
                            break;
                    }
                }

                endpoint = null;
                responce = null;
                content = null;
                resultFromServer = null;
            }
            catch(Exception e)
            {
                printMessageMethods.Invoke($"{OPERATION_FAILED}");
                throw new Exception($"");
            }

            if(environmentProxy != null)
            {
                try
                {
                    WebProxy webProxy = new WebProxy
                    {
                        Address = environmentProxy.FullProxyAddressUri,
                        BypassProxyOnLocal = false,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(environmentProxy.Username, environmentProxy.Password)
                    };

                    HttpClientHandler clientHandler = new HttpClientHandler
                    {
                        UseProxy = true,
                        Proxy = webProxy,
                        UseCookies = true,
                        CookieContainer = ProxyParseCookieContainer = new CookieContainer(),
                        AutomaticDecompression = DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | DecompressionMethods.Brotli
                    };

                    // Disable SSL verification
                    clientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                    ProxyParseHttpClient = new HttpClient(clientHandler, true)
                    {
                        Timeout = TimeSpan.FromSeconds(30),
                    };

                    printMessageMethods.Invoke($"{OPERATION_SUCCESSFUL} ({environmentProxy.FullProxyAddress})");
                }
                catch (Exception e)
                {
                    printMessageMethods.Invoke($"{OPERATION_FAILED}");
                    throw new Exception($"");
                }
            }
            else
            {
                try
                {
                    HttpClientHandler clientHandler = new HttpClientHandler
                    {
                        UseProxy = false,
                        UseCookies = true,
                        CookieContainer = ProxyParseCookieContainer = new CookieContainer(),
                        AutomaticDecompression = DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | DecompressionMethods.Brotli
                    };

                    // Disable SSL verification
                    clientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                    ProxyParseHttpClient = new HttpClient(clientHandler, true)
                    {
                        Timeout = TimeSpan.FromSeconds(30),
                    };

                    printMessageMethods.Invoke($"{OPERATION_SUCCESSFUL} (Без прокси)");
                }
                catch (Exception e)
                {
                    printMessageMethods.Invoke($"{OPERATION_FAILED}");
                    throw new Exception($"");
                }
            }
        }

        // Метод обновления токена 
        private static async Task clientRenewAccessToken()
        {
            Uri? endpoint = null;
            JsonContent? jsonContent = null;
            HttpResponseMessage? responce = null;
            string? content = null;

            endpoint = new Uri($"{client!.SERVER_HOST}/api/client/session/renew");
            jsonContent = JsonContent.Create(client);

            ServerConnections.serverhttpConnection!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", client.ReadEnvironmentVariable("TOKEN", EnvironmentVariableTarget.Process));
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = endpoint,
                Content = jsonContent
            };

            // remove after
            request.Headers.Add("Remote_Addr", "192.168.0.1");
            // remove after

            responce = await ServerConnections.serverhttpConnection!.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            content = await responce.Content.ReadAsStringAsync();

            if (!responce.IsSuccessStatusCode)
            {
                throw new Exception($"Не удалось обновить токен доступа\n{content}");
            }

            if (content == null)
            {
                throw new Exception($"Ошибка при обновлении токена сессии\ncontent == null");
            }

            Dictionary<string, string>? resultFromServer = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            if (resultFromServer == null)
            {
                throw new Exception($"Ошибка при обновлении(чтения) токена сессии\nresultFromServer == null");
            }

            var TOKEN = resultFromServer!.Where(item => item.Key == "token").FirstOrDefault();

            if (String.IsNullOrEmpty(TOKEN.Value))
            {
                throw new Exception($"Ошибка при получении токена сессии\nString.IsNullOrEmpty(TOKEN.Key)");
            }

            // Запись токена сессии
            try
            {
                client.WriteEnvironmentVariable(nameof(TOKEN), TOKEN.Value, EnvironmentVariableTarget.Process);
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка при записи токена сессии\n" + e.Message);
            }

            await ServerConnections.serverHubConnection!.StopAsync();
            await ServerConnections.serverHubConnection!.StartAsync();

            endpoint = null;
            jsonContent = null;
            responce = null;
            content = null;
        }

        // метод ожидающий очищения бота от предыдущего задания перед выполнением следующего
        private static async Task waitForDisposing()
        {
            while (BOT_IS_DISPOSING)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        // Метод блокирующий выполнение вызываемых сервером методов если клиент занят
        private async static Task clientIsBusy()
        {
            try
            {
                await ServerConnections.serverHubConnection!.SendAsync("client_is_busy_now", client.ID);
            }
            catch { }
        }
        private async static Task clientIsBusy(string task_id)
        {
            try
            {
                await ServerConnections.serverHubConnection!.SendAsync("client_is_busy_now", client.ID, task_id);
            }
            catch { }
        }
        private async static Task clientInternalError(string error_message)
        {
            try
            {
                await ServerConnections.serverHubConnection!.SendAsync("client_internal_error", error_message);
            }
            catch { }
        }
    }
}