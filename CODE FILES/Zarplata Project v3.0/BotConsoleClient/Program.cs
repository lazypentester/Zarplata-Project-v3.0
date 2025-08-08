using CommonModels.Client;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithoutAuth;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.SelectiveTaskWithAuth;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using SocpublicCom.Classes.Public;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static ClientInitialization.Classes.Public.InitDelegates;
using static CommonModels.ProjectTask.ProjectTaskEnums;

namespace BotConsoleClient
{
    internal class Program
    {
        static EarningSiteWorkBotClient client = new EarningSiteWorkBotClient();
        static PrintMessage printMessageMethods = new Init().PrintClientMessage;
        static bool CLIENT_IS_BUSY_NOW = false;

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

            Thread.Sleep(Timeout.Infinite); // еще раз почитать правильно ли так делать
        }

        static void RegisterTaskHubMethods()
        {
            #region Исполняемые методы SignalR

            #region Platforms task SocpublicCom
            ServerConnections.serverHubConnection!.On<GroupSelectiveTaskWithAuth>("do_new_platform_socpubliccom_task", async (task) =>
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
                //try
                //{
                //    await ServerConnections.serverHubConnection!.SendAsync("set_bot_status_to_atwork");
                //}
                //catch { }

                // запуск задания
                try
                {
                    await new DoPlatformTask().Do(client, task, ServerConnections.serverHubConnection!, ServerConnections.serverhttpConnection!);
                }
                catch (Exception e)
                {
                    try
                    {
                        await DoPlatformTask.TaskChangedStatus(TaskResultStatus.Error, e);
                    }
                    catch { }
                }
            });

            ServerConnections.serverHubConnection!.On<GroupSelectiveTaskWithoutAuth>("do_new_platform_socpubliccom_task", async (task) =>
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
                    await new DoPlatformTask().Do(client, task, ServerConnections.serverHubConnection!, ServerConnections.serverhttpConnection!);
                }
                catch (Exception e)
                {
                    try
                    {
                        await DoPlatformTask.TaskChangedStatus(TaskResultStatus.Error, e);
                    }
                    catch { }
                }
            });

            ServerConnections.serverHubConnection!.On<SocpublicComAutoregTask>("do_new_platform_socpubliccom_autoreg_task", async (task) =>
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
                //try
                //{
                //    await ServerConnections.serverHubConnection!.SendAsync("set_bot_status_to_atwork");
                //}
                //catch { }

                // запуск задания
                try
                {
                    await new DoPlatformTask().Do(client, task, ServerConnections.serverHubConnection!, ServerConnections.serverhttpConnection!);
                }
                catch (Exception e)
                {
                    try
                    {
                        await DoPlatformTask.TaskChangedStatus(TaskResultStatus.Error, e);
                    }
                    catch { }
                }
            });
            ServerConnections.serverHubConnection!.On<WithdrawMoneyGroupSelectiveTaskWithAuth>("do_new_platform_socpubliccom_withdrawal_task", async (task) =>
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
                //try
                //{
                //    await ServerConnections.serverHubConnection!.SendAsync("set_bot_status_to_atwork");
                //}
                //catch { }

                // запуск задания
                try
                {
                    await new DoPlatformTask().Do(client, task, ServerConnections.serverHubConnection!, ServerConnections.serverhttpConnection!);
                }
                catch (Exception e)
                {
                    try
                    {
                        await DoPlatformTask.TaskChangedStatus(TaskResultStatus.Error, e);
                    }
                    catch { }
                }
            });

            ServerConnections.serverHubConnection!.On<string>("dispose_platform_socpubliccom_and_set_new_token", async (token) =>
            {
                //if (CLIENT_IS_BUSY_NOW)
                //{
                //    await clientIsBusy();
                //    return;
                //}

                BOT_IS_DISPOSING = true;

                try
                {
                    DoPlatformTask.DisposeTask();
                    CLIENT_IS_BUSY_NOW = false;
                }
                catch (Exception e)
                {
                    try
                    {
                        await clientInternalError(e.Message);
                    }
                    catch { }
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

            #endregion
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