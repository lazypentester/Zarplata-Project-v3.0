using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Database.Services;
using Server.Attributes.Authorization;
using CommonModels.Client;
using Server.ResolveCaptcha.CaptchaGuruRu;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.Captcha;
using CommonModels.ProjectTask.ProxyCombiner.Models;
using CommonModels.ProjectTask.ProxyCombiner;
using System.Collections.Generic;
using CommonModels.ProjectTask.Platform;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.SpecialCombineTask;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.DefaultCombineTask;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.SpecialCombineTask.Models;
using static CommonModels.Client.Client;
using CommonModels.ProjectTask.EarningSite;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.SelectiveTaskWithAuth;
using CommonModels.Client.Models;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using TaskStatus = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using MongoDB.Bson;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using System.Threading.Tasks;
using CommonModels.ProjectTask;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney;
using static CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney.WithdrawMoneyGroupSelectiveTaskWithAuth;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.Models.WithdrawalOfMoneyModels;
using static CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth.SelectiveTask.CheckCareerLadder;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth.SelectiveTask.TaskVisitWithoutTimer;
using static CommonModels.ProjectTask.Platform.EarningSiteTaskUrls;
using System.Linq;

namespace Server.Hubs
{
    public class ClientHub : Hub
    {
        private readonly ClientsService clientsService;
        private readonly SiteParseBalancerService siteParseBalancersService;
        private readonly AccountReservedProxyService accountReservedProxiesService;
        private readonly ProxyTasksService proxyTasksService;
        private readonly ProxyTasksErorrsLog proxyTasksErorrsLog;
        private readonly EarnSiteTasksService earnSiteTasksService;
        private readonly ClientsService clientsBotService;
        private readonly SocpublicAccountsService socpublicAccountsService;
        private readonly RunTasksSettingsService runTasksSettingsService;
        private readonly PlatformInternalAccountTaskService platformInternalAccountTaskService;
        private readonly IHubContext<ManagementHub> ManagementHubContext;

        public ClientHub(ClientsService clientsService, SiteParseBalancerService siteParseBalancersService, AccountReservedProxyService accountReservedProxiesService, IHubContext<ManagementHub> managementHubContext, ProxyTasksService proxyTasksService, EarnSiteTasksService earnSiteTasksService, ClientsService clientsBotService, SocpublicAccountsService socpublicAccountsService, ProxyTasksErorrsLog proxyTasksErorrsLog, RunTasksSettingsService runTasksSettingsService, PlatformInternalAccountTaskService platformInternalAccountTaskService)
        {
            this.clientsService = clientsService;
            this.siteParseBalancersService = siteParseBalancersService;
            this.accountReservedProxiesService = accountReservedProxiesService;
            ManagementHubContext = managementHubContext;
            this.proxyTasksService = proxyTasksService;
            this.earnSiteTasksService = earnSiteTasksService;
            this.clientsBotService = clientsBotService;
            this.socpublicAccountsService = socpublicAccountsService;
            this.proxyTasksErorrsLog = proxyTasksErorrsLog;
            this.runTasksSettingsService = runTasksSettingsService;
            this.platformInternalAccountTaskService = platformInternalAccountTaskService;
        }

        #region МЕТОДЫ ВЫЗЫВАЕМЫЕ ПРИ ПОДКЛЮЧЕНИИ И ОТКЛЮЧЕНИИ КЛИЕНТОВ

        public override async Task OnConnectedAsync()
        {
            if (Context.UserIdentifier != null)
            {
                // update Bot status
                await clientsService.UpdateAsync(Context.UserIdentifier, "Status", (int)ClientStatus.Connected);

                Console.WriteLine($"Клиент id#{Context.UserIdentifier} подключился к хабу");
            }
            else
            {
                Console.WriteLine($"Клиент anonymous#{Context.ConnectionId} подключился к хабу");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            //await File.WriteAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), $"{Context.UserIdentifier}_bot_log_file.txt"), "\n"+$"{exception?.Message}");

            if (Context.UserIdentifier != null)
            {
                // Возвращение задания на чекинг/парсинг прокси если бот вылетел по ошибке, для того чтобы бот platformworker не повис в вечном ожидании результата по проксям
                try
                {
                    List<dynamic> proxyTasksExecuting = await proxyTasksService.GetOneDisconnectedAsync();
                    
                    if(proxyTasksExecuting.Count > 0)
                    {
                        dynamic? proxyTaskDisconnected = proxyTasksExecuting.Where(x => x.ExecutorId == Context.UserIdentifier).FirstOrDefault();

                        if (proxyTaskDisconnected != null && proxyTaskDisconnected as SpecialCombineTask != null)
                        {
                            SpecialCombineTask normalTask = proxyTaskDisconnected!;

                            // поиск свбодного прокси бота и выдача ему задания от отключившигося прокси бота

                            List<ModelClient> bots = await clientsService.GetAsync();

                            List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.ProxyCombineBot).ToList();

                            if (freeBots.Count > 0)
                            {
                                // send Task to bot
                                await Clients.User(freeBots[0].ID!).SendAsync($"do_new_proxycombiner_{normalTask.InternalType.ToString().ToLower()}_task", normalTask);

                                // update Bot status
                                await clientsService.UpdateAsync(freeBots[0].ID!, "Status", (int)ClientStatus.AtWork);

                                // update task status
                                // proxyTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                                // update Task executorId
                                await proxyTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", freeBots[0].ID!);
                            }
                            else
                            {
                                // update task status
                                await proxyTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Created);
                            }
                        }
                        else if(proxyTaskDisconnected != null && proxyTaskDisconnected as CheckTask != null)
                        {
                            CheckTask normalTask = proxyTaskDisconnected!;

                            // поиск свбодного прокси бота и выдача ему задания от отключившигося прокси бота

                            List<ModelClient> bots = await clientsService.GetAsync();

                            List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.ProxyCombineBot).ToList();

                            if (freeBots.Count > 0)
                            {
                                // send Task to bot
                                await Clients.User(freeBots[0].ID!).SendAsync($"do_new_proxycombiner_{normalTask.InternalType.ToString().ToLower()}_task", normalTask);

                                // update Bot status
                                await clientsService.UpdateAsync(freeBots[0].ID!, "Status", (int)ClientStatus.AtWork);

                                // update task status
                                // proxyTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                                // update Task executorId
                                await proxyTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", freeBots[0].ID!);
                            }
                            else
                            {
                                // update task status
                                await proxyTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Created);
                            }
                        }
                    }
                }
                catch { }

                try
                {
                    // Возвращение задания в обычную очередь(платформ таск)
                    List<dynamic> earnTasksByThisBot = await earnSiteTasksService.GetTasksByExecutorIdAsync(Context.UserIdentifier);

                    if(earnTasksByThisBot.Count > 0)
                    {
                        dynamic? earnTaskDisconnected = earnTasksByThisBot.Where(x => x.Status != TaskStatus.Created && x.Status != TaskStatus.Done).FirstOrDefault();

                        if (earnTaskDisconnected != null)
                        {
                            if (earnTaskDisconnected as GroupSelectiveTaskWithAuth != null)
                            {
                                GroupSelectiveTaskWithAuth normalTask = earnTaskDisconnected!;

                                // поиск свбодного earn бота и выдача ему задания от отключившигося earn бота

                                List<ModelClient> bots = await clientsService.GetAsync();

                                List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.EarningSiteBot).ToList();

                                if (freeBots.Count > 0)
                                {
                                    // send Task to bot
                                    await Clients.User(freeBots[0].ID!).SendAsync($"do_new_platform_socpubliccom_task", normalTask);

                                    // update task status
                                    await earnSiteTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Started);

                                    // update Bot status
                                    await clientsBotService.UpdateAsync(freeBots[0].ID!, "Status", (int)ClientStatus.AtWork);

                                    // update Task executorId
                                    await earnSiteTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", freeBots[0].ID!);
                                }
                                else
                                {
                                    // update task status
                                    await earnSiteTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Created);
                                } 
                            }
                            else if (earnTaskDisconnected as SocpublicComAutoregTask != null)
                            {
                                SocpublicComAutoregTask normalTask = earnTaskDisconnected!;

                                // поиск свбодного earn бота и выдача ему задания от отключившигося earn бота

                                List<ModelClient> bots = await clientsService.GetAsync();

                                List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.EarningSiteBot).ToList();

                                if (freeBots.Count > 0)
                                {
                                    // send Task to bot
                                    await Clients.User(freeBots[0].ID!).SendAsync($"do_new_platform_socpubliccom_autoreg_task", normalTask);

                                    // update task status
                                    await earnSiteTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Started);

                                    // update Bot status
                                    await clientsBotService.UpdateAsync(freeBots[0].ID!, "Status", (int)ClientStatus.AtWork);

                                    // update Task executorId
                                    await earnSiteTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", freeBots[0].ID!);
                                }
                                else
                                {
                                    // update task status
                                    await earnSiteTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Created);
                                }
                            }
                            else if (earnTaskDisconnected as WithdrawMoneyGroupSelectiveTaskWithAuth != null)
                            {
                                WithdrawMoneyGroupSelectiveTaskWithAuth normalTask = earnTaskDisconnected!;

                                if(normalTask.IsTaskCreatedOnPlatformBySlave.HasValue && !normalTask.IsTaskCreatedOnPlatformBySlave.Value)
                                {
                                    // поиск свбодного earn бота и выдача ему задания от отключившигося earn бота

                                    List<ModelClient> bots = await clientsService.GetAsync();

                                    List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.EarningSiteBot).ToList();

                                    if (freeBots.Count > 0)
                                    {
                                        // send Task to bot
                                        await Clients.User(freeBots[0].ID!).SendAsync($"do_new_platform_socpubliccom_withdrawal_task", normalTask);

                                        // update task status
                                        await earnSiteTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Started);

                                        // update Bot status
                                        await clientsBotService.UpdateAsync(freeBots[0].ID!, "Status", (int)ClientStatus.AtWork);

                                        // update Task executorId
                                        await earnSiteTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", freeBots[0].ID!);
                                    }
                                    else
                                    {
                                        // update task status
                                        await earnSiteTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Created);
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }

                // Удаление клиента из базы данных 
                // update Bot status to disconnected
                try
                {
                    // update Bot status
                    await clientsService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.Disconnected);
                    await clientsService.UpdateAsync(Context.UserIdentifier!, "DisconnectDateTime", DateTime.UtcNow);
                    //await clientsService.DeleteAsync(Context.UserIdentifier);
                }
                catch { }

                // Уведомление менеджерских приложений о закрытом боте (об отлюченном боте)
                try
                {
                    //await ManagementHubContext.Clients.All.SendAsync("bot_closed", Context.UserIdentifier);
                }
                catch { }

                Console.WriteLine($"Клиент id#{Context.UserIdentifier} отключился от хаба");
            }
            else
            {
                Console.WriteLine($"Клиент anonymous#{Context.ConnectionId} отключился от хаба");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task change_bot_status_from_connected_to_free()
        {
            //remove after
            if (Context.UserIdentifier == null)
            {
                File.Create(Path.Combine(Directory.GetCurrentDirectory(), "Context.UserIdentifier == null.error")); 
            }

            // update Bot status
            await clientsService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.Free);
        }

        public async Task check_on_need_to_give_task_to_connected_bot(BotRole role)
        {
            List<RunTasksSettings> run_settings = await runTasksSettingsService.GetAsync();

            if (run_settings.Count == 1 && run_settings[0].TasksIsRunning != null && run_settings[0].TasksIsRunning!.Value && role == BotRole.EarningSiteBot)
            {
                // get task
                try
                {
                    //List<dynamic> tasks = await earnSiteTasksService.GetAsync();
                    //dynamic? task = tasks!.Where(t => t.Type == CommonModels.ProjectTask.ProjectTaskEnums.TaskType.EarningSiteWork && t.Status == CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created).FirstOrDefault();

                    //GroupSelectiveTaskWithAuth normalTask = task!;
                    dynamic? _task = null;

                    if (run_settings[0].CurrentLaunchedTaskType == LaunchTaskType.EarningMoney)
                    {
                        _task = await earnSiteTasksService.GetOneCreatedAsync();

                        if (_task as GroupSelectiveTaskWithAuth != null)
                        {
                            GroupSelectiveTaskWithAuth? normalTask = _task;

                            if (normalTask != null)
                            {
                                // send Task to bot
                                await Clients.Caller.SendAsync("do_new_platform_socpubliccom_task", normalTask);

                                // update task status
                                await earnSiteTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                                // update Bot status
                                await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);

                                // update Task executorId
                                await earnSiteTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", Context.UserIdentifier!);
                            }  
                        }
                    }
                    else if(run_settings[0].CurrentLaunchedTaskType == LaunchTaskType.WithdrawingMoney)
                    {
                        _task = await earnSiteTasksService.GetOneCreatedWirthdrawForSlaveAsync();

                        if (_task as WithdrawMoneyGroupSelectiveTaskWithAuth != null)
                        {
                            WithdrawMoneyGroupSelectiveTaskWithAuth? normalTask = _task;

                            if (normalTask != null)
                            {
                                // send Task to bot
                                await Clients.Caller.SendAsync("do_new_platform_socpubliccom_withdrawal_task", normalTask);

                                // update task status
                                await earnSiteTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                                // update Bot status
                                await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);

                                // update Task executorId
                                await earnSiteTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", Context.UserIdentifier!);
                            }
                        }
                    }
                    else if (run_settings[0].CurrentLaunchedTaskType == LaunchTaskType.AutoregAccounts)
                    {
                        _task = await earnSiteTasksService.GetOneCreatedAsync();

                        if (_task as SocpublicComAutoregTask != null)
                        {
                            SocpublicComAutoregTask? normalTask = _task;

                            if (normalTask != null)
                            {
                                // send Task to bot
                                await Clients.Caller.SendAsync("do_new_platform_socpubliccom_autoreg_task", normalTask);

                                // update task status
                                await earnSiteTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                                // update Bot status
                                await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);

                                // update Task executorId
                                await earnSiteTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", Context.UserIdentifier!);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            else if(run_settings.Count == 1 && run_settings[0].TasksIsRunning != null && run_settings[0].TasksIsRunning!.Value && role == BotRole.ProxyCombineBot)
            {
                // get task
                try
                {
                    List<dynamic>? tasks = await proxyTasksService.GetAsync();

                    if (tasks?.Count > 0)
                    {
                        dynamic? task_ = tasks!.Where(t => t.Type == CommonModels.ProjectTask.ProjectTaskEnums.TaskType.ProxyCombineWork && t.Status == CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created).FirstOrDefault();

                        if (task_ != null && task_ as SpecialCombineTask != null)
                        {
                            SpecialCombineTask normalTask = task_!;

                            if (normalTask != null)
                            {
                                // send Task to bot
                                await Clients.Caller.SendAsync($"do_new_proxycombiner_{normalTask.InternalType.ToString().ToLower()}_task", normalTask);

                                // update task status
                                await proxyTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                                // update Bot status
                                await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);

                                // update Task executorId
                                await proxyTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", Context.UserIdentifier!);
                            }
                        }
                        else if (task_ != null && task_ as CheckTask != null)
                        {
                            CheckTask normalTask = task_!;

                            if (normalTask != null)
                            {
                                // send Task to bot
                                await Clients.Caller.SendAsync($"do_new_proxycombiner_{normalTask.InternalType.ToString().ToLower()}_task", normalTask);

                                // update task status
                                await proxyTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                                // update Bot status
                                await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);

                                // update Task executorId
                                await proxyTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", Context.UserIdentifier!);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task set_bot_status_to_atwork()
        {
            // update Bot status
            await clientsService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);
        }

        public async Task addClientToHubGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        #endregion

        #region PROXY COMBINER

        //delete after
        //[HubAuthorizeIP]
        public async Task check_platform_account_history_proxyTEST()
        {
            List<CheckedProxy> proxies = new List<CheckedProxy>()
            {
                new CheckedProxy(new ParsedProxy("172.67.181.138", "80")),
                new CheckedProxy(new ParsedProxy("202.86.138.18", "8080")),
                new CheckedProxy(new ParsedProxy("50.217.226.40", "80")),
                new CheckedProxy(new ParsedProxy("43.157.10.238", "8888")),
                new CheckedProxy(new ParsedProxy("172.67.181.138", "80")),
                new CheckedProxy(new ParsedProxy("203.24.103.226", "80")),
                new CheckedProxy(new ParsedProxy("203.22.223.44", "80")),
                new CheckedProxy(new ParsedProxy("64.225.8.121", "9989")),
                new CheckedProxy(new ParsedProxy("20.206.106.192", "8123")),
                new CheckedProxy(new ParsedProxy("89.43.31.134", "3128")),
                new CheckedProxy(new ParsedProxy("162.55.63.236", "8080")),
                new CheckedProxy(new ParsedProxy("20.24.43.214", "8123")),
                new CheckedProxy(new ParsedProxy("20.111.54.16", "8123")),
                new CheckedProxy(new ParsedProxy("54.37.137.112", "8118")),
                new CheckedProxy(new ParsedProxy("157.245.27.9", "4000")),
                new CheckedProxy(new ParsedProxy("24.199.106.243", "3128")),
                new CheckedProxy(new ParsedProxy("192.252.209.155", "14455")),
                new CheckedProxy(new ParsedProxy("98.162.25.16", "4145")),
                new CheckedProxy(new ParsedProxy("185.87.121.5", "8975")),
                new CheckedProxy(new ParsedProxy("5.182.26.129", "4145")),
                new CheckedProxy(new ParsedProxy("104.248.29.72", "2333")),
                new CheckedProxy(new ParsedProxy("89.208.105.58", "1488")),
                new CheckedProxy(new ParsedProxy("193.141.65.48", "8975")),
                new CheckedProxy(new ParsedProxy("98.162.25.23", "4145")),
                new CheckedProxy(new ParsedProxy("104.37.135.145", "4145")),
                new CheckedProxy(new ParsedProxy("63.53.204.178", "9813")),
                new CheckedProxy(new ParsedProxy("139.59.1.14", "1080")),
                new CheckedProxy(new ParsedProxy("203.23.104.31", "80")),
                new CheckedProxy(new ParsedProxy("185.238.228.202", "80")),
                new CheckedProxy(new ParsedProxy("203.22.223.135", "80")),
                new CheckedProxy(new ParsedProxy("203.34.28.135", "80")),
                new CheckedProxy(new ParsedProxy("45.8.105.62", "80")),
                new CheckedProxy(new ParsedProxy("45.8.105.187", "80")),
                new CheckedProxy(new ParsedProxy("185.162.229.76", "80")),
                new CheckedProxy(new ParsedProxy("203.23.104.45", "80")),
                new CheckedProxy(new ParsedProxy("203.23.104.180", "80")),
            };

            CheckRequirements requirements = new CheckRequirements(
                "http://socpublic.com",
                "https://socpublic.com",
                new CheckedProxy.CPProtocol[] { CheckedProxy.CPProtocol.http, CheckedProxy.CPProtocol.socks4, CheckedProxy.CPProtocol.socks5 },
                new CheckedProxy.SupportsSecureConnectionState[] { CheckedProxy.SupportsSecureConnectionState.Support, CheckedProxy.SupportsSecureConnectionState.NotSupport, CheckedProxy.SupportsSecureConnectionState.Undefined },
                new CheckedProxy.AnonymousState[] { CheckedProxy.AnonymousState.Anonymous },
                TimeSpan.FromSeconds(5).Milliseconds,
                new CheckedProxy.CPWorkState[] { CheckedProxy.CPWorkState.Work }
                );

            await check_platform_account_history_proxy(proxies, requirements);
        }
        //delete after
        public async Task get_platform_account_new_proxyTEST()
        {
            CheckRequirements requirements = new CheckRequirements(
                "http://socpublic.com",
                "https://socpublic.com",
                new CheckedProxy.CPProtocol[] { CheckedProxy.CPProtocol.http, CheckedProxy.CPProtocol.socks4, CheckedProxy.CPProtocol.socks5 },
                new CheckedProxy.SupportsSecureConnectionState[] { CheckedProxy.SupportsSecureConnectionState.Support, CheckedProxy.SupportsSecureConnectionState.NotSupport, CheckedProxy.SupportsSecureConnectionState.Undefined },
                new CheckedProxy.AnonymousState[] { CheckedProxy.AnonymousState.Anonymous },
                TimeSpan.FromSeconds(5).Milliseconds,
                new CheckedProxy.CPWorkState[] { CheckedProxy.CPWorkState.Work }
                );

            //await get_platform_account_new_proxy(requirements);
        }
        // delete after
        public async Task parseTest()
        {
            ParseRequirements requirements = new ParseRequirements(10000000);

            ParseTask task = new ParseTask("007", CommonModels.ProjectTask.ProjectTaskEnums.TaskFrom.Bot, CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created, requirements);

            await Clients.All.SendAsync($"do_new_proxycombiner_{task.InternalType.ToString().ToLower()}_task", task);
        }
        // delete after
        public async Task defaultCombineTest()
        {
            ParseRequirements prequirements = new ParseRequirements(500);
            CheckRequirements crequirements = new CheckRequirements(
                "http://socpublic.com",
                "https://socpublic.com",
                new CheckedProxy.CPProtocol[] { CheckedProxy.CPProtocol.http, CheckedProxy.CPProtocol.socks4, CheckedProxy.CPProtocol.socks5 },
                new CheckedProxy.SupportsSecureConnectionState[] { CheckedProxy.SupportsSecureConnectionState.Support, CheckedProxy.SupportsSecureConnectionState.NotSupport, CheckedProxy.SupportsSecureConnectionState.Undefined },
                new CheckedProxy.AnonymousState[] { CheckedProxy.AnonymousState.Anonymous },
                TimeSpan.FromSeconds(5).Milliseconds,
                new CheckedProxy.CPWorkState[] { CheckedProxy.CPWorkState.Work }
                );

            DefaultCombineTask task = new DefaultCombineTask("007", CommonModels.ProjectTask.ProjectTaskEnums.TaskFrom.Bot, CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created, prequirements, crequirements);
            await Clients.All.SendAsync($"do_new_proxycombiner_{task.InternalType.ToString().ToLower()}_task", task);
        }




        public async Task check_platform_account_history_proxy(List<CheckedProxy> proxies, CheckRequirements checkRequirements)
        {
            // request to proxy-combiner service with data proxies and checkRequirements

            // get id free proxycombiner-bot from database, if no one free bots - add task to queue

            try
            {
                List<ParsedProxy> parsedProxies = new List<ParsedProxy>();
                foreach (CheckedProxy proxy in proxies)
                {
                    parsedProxies.Add(proxy.proxy);
                }

                //Context.UserIdentifier! == bot.id
                CheckTask task = new CheckTask(Context.UserIdentifier!, CommonModels.ProjectTask.ProjectTaskEnums.TaskFrom.Bot, CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created, parsedProxies, checkRequirements)
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    QueuePosition = 0,
                    Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
                    ResultStatus = TaskResultStatus.Unknown,
                    ActionAfterFinish = TaskActionAfterFinish.Renew,
                };

                await proxyTasksService.CreateAsync(task);


                // give task to bot
                List<ModelClient> bots = await clientsService.GetAsync();

                List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.ProxyCombineBot).ToList();

                if (freeBots.Count > 0)
                {
                    // send Task to bot
                    await Clients.User(freeBots[0].ID!).SendAsync($"do_new_proxycombiner_{task.InternalType.ToString().ToLower()}_task", task);

                    // update Bot status
                    await clientsService.UpdateAsync(freeBots[0].ID!, "Status", (int)ClientStatus.AtWork);

                    // update task status
                    await proxyTasksService.UpdateAsync(task.Id!, nameof(task.Status), (int)TaskStatus.Executing);

                    // update Task executorId
                    await proxyTasksService.UpdateAsync(task.Id!, "ExecutorId", freeBots[0].ID!);
                }

            }
            catch (Exception ex)
            {
            }
        }
        public async Task get_platform_account_new_proxy(EarningSiteWorkBotClient client, Account? account, CheckRequirements checkRequirements)
        {
            // request to proxy-combiner service with data checkRequirements

            try
            {
                ParseRequirements prequirements = new ParseRequirements(100);
                string? id = account is null ? null : account.Id;
                SpecialRequirements specialRequirements = new SpecialRequirements(id, EarningSiteTaskEnums.EarningSiteEnum.SocpublicDotCom);


                SpecialCombineTask task = new SpecialCombineTask(client.ID!, CommonModels.ProjectTask.ProjectTaskEnums.TaskFrom.Bot, CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created, prequirements, checkRequirements, specialRequirements)
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    QueuePosition = 0,
                    Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
                    ResultStatus = TaskResultStatus.Unknown,
                    ActionAfterFinish = TaskActionAfterFinish.Renew,
                };

                await proxyTasksService.CreateAsync(task);

                // give task to bot
                List<ModelClient> bots = await clientsService.GetAsync();

                List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.ProxyCombineBot).ToList();

                if (freeBots.Count > 0)
                {
                    // send Task to bot
                    await Clients.User(freeBots[0].ID!).SendAsync($"do_new_proxycombiner_{task.InternalType.ToString().ToLower()}_task", task);

                    // update Bot status
                    await clientsService.UpdateAsync(freeBots[0].ID!, "Status", (int)ClientStatus.AtWork);

                    // update task status
                    await proxyTasksService.UpdateAsync(task.Id!, nameof(task.Status), (int)TaskStatus.Executing);

                    // update Task executorId
                    await proxyTasksService.UpdateAsync(task.Id!, "ExecutorId", freeBots[0].ID!);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<List<SiteParseBalancer>?> proxy_combiner_get_site_parse_balancer_collection(SpecialCombineTask task, ProxyCombineBotClient client)
        {
            List<SiteParseBalancer>? data = null;

            try
            {
                data = await siteParseBalancersService.GetAsync();
            }
            catch(Exception e)
            {
                // print exception in log
            }

            return data;
        }
        public async Task proxy_combiner_update_site_parse_balancer_collection(SiteParseBalancer site)
        {
            try
            {
                string command = "$set";

                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { nameof(site.CountParseSite), site.CountParseSite },
                    { nameof(site.LastTimeParseSite), site.LastTimeParseSite },
                    { nameof(site.LastClientIdParseSite), site.LastClientIdParseSite }
                };

                await siteParseBalancersService.UpdateAsync(site.Id!, data, command);
            }
            catch (Exception e)
            {
               
                // print exception in log
            }
        }
        public async Task<List<AccountReservedProxy>?> proxy_combiner_get_account_reserved_proxy_collection(ProxyCombineBotClient client, EarningSiteTaskEnums.EarningSiteEnum reservedPlatform)
        {
            List<AccountReservedProxy>? data = null;

            try
            {
                data = await accountReservedProxiesService.GetAsync(reservedPlatform);
            }
            catch (Exception e)
            {
                // print exception in log
            }

            return data;
        }
        public async Task proxy_combiner_push_checked_proxy_to_account_reserved_proxy_collection(ProxyCombineBotClient client, AccountReservedProxy accountReservedProxy)
        {
            try
            {
                await accountReservedProxiesService.CreateAsync(accountReservedProxy);
            }
            catch (Exception e)
            {
                //pushed = false;
                // print exception in log
            }
        }
        public async Task proxy_combiner_push_checked_proxy_autoreg_to_account_reserved_proxy_collection(ProxyCombineBotClient client, AccountReservedProxy accountReservedProxy)
        {
            try
            {
                await accountReservedProxiesService.CreateAsync(accountReservedProxy);
            }
            catch (Exception e)
            {
                //pushed = false;
                // print exception in log
            }
        }

        public async Task proxy_combiner_check_task_status_changed(CheckTask task)
        {
            // dispose bot task
            await Clients.Caller.SendAsync("dispose_proxycombiner_and_set_new_token", "token herokin)");

            // update Bot status
            await clientsService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.Free);

            // send Task result to bot
            await Clients.User(task.AuthorId!).SendAsync($"checked_platform_account_history_proxy", task.result);

            // update task status
            await proxyTasksService.UpdateAsync(task.Id!, "Status", (int)TaskStatus.Done);


            // get task
            try
            {
                List<dynamic>? tasks = await proxyTasksService.GetAsync();
                dynamic? task_ = tasks!.Where(t => t.Type == CommonModels.ProjectTask.ProjectTaskEnums.TaskType.ProxyCombineWork && t.Status == CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created).FirstOrDefault();

                if (task_ != null && task_ as SpecialCombineTask != null)
                {
                    SpecialCombineTask normalTask = task_!;

                    if (normalTask != null)
                    {
                        // send Task to bot
                        await Clients.Caller.SendAsync($"do_new_proxycombiner_{normalTask.InternalType.ToString().ToLower()}_task", normalTask);

                        // update task status
                        await proxyTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                        // update Bot status
                        await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);

                        // update Task executorId
                        await proxyTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", Context.UserIdentifier!);
                    }
                }
                else if(task_ != null && task_ as CheckTask != null)
                {
                    CheckTask normalTask = task_!;

                    if (normalTask != null)
                    {
                        // send Task to bot
                        await Clients.Caller.SendAsync($"do_new_proxycombiner_{normalTask.InternalType.ToString().ToLower()}_task", normalTask);

                        // update task status
                        await proxyTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                        // update Bot status
                        await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);

                        // update Task executorId
                        await proxyTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", Context.UserIdentifier!);
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            //await ManagementHubContext.Clients.All.SendAsync("proxy_combiner_task_status_changed", task);
        }
        public async Task proxy_combiner_parse_task_status_changed(ParseTask task)
        {
            await ManagementHubContext.Clients.All.SendAsync("proxy_combiner_task_status_changed", task);
        }
        public async Task proxy_combiner_specialcombine_task_status_changed(SpecialCombineTask task)
        {
            // dispose bot task
            await Clients.Caller.SendAsync("dispose_proxycombiner_and_set_new_token", "token herokin)");

            if (task.result != null)
            {
                // update Bot status
                await clientsService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.Free);

                // send Task result to bot
                await Clients.User(task.AuthorId!).SendAsync($"new_platform_account_proxy", task.result);

                // update task status
                await proxyTasksService.UpdateAsync(task.Id!, nameof(task.Status), (int)TaskStatus.Done);

                // get task
                try
                {
                    List<dynamic>? tasks = await proxyTasksService.GetAsync();

                    if (tasks?.Count > 0)
                    {
                        dynamic? task_ = tasks!.Where(t => t.Type == CommonModels.ProjectTask.ProjectTaskEnums.TaskType.ProxyCombineWork && t.Status == CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created).FirstOrDefault();

                        if (task_ != null && task_ as SpecialCombineTask != null)
                        {
                            SpecialCombineTask normalTask = task_!;

                            if (normalTask != null)
                            {
                                // send Task to bot
                                await Clients.Caller.SendAsync($"do_new_proxycombiner_{normalTask.InternalType.ToString().ToLower()}_task", normalTask);

                                // update task status
                                await proxyTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                                // update Bot status
                                await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);

                                // update Task executorId
                                await proxyTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", Context.UserIdentifier!);
                            }
                        }
                        else if (task_ != null && task_ as CheckTask != null)
                        {
                            CheckTask normalTask = task_!;

                            if (normalTask != null)
                            {
                                // send Task to bot
                                await Clients.Caller.SendAsync($"do_new_proxycombiner_{normalTask.InternalType.ToString().ToLower()}_task", normalTask);

                                // update task status
                                await proxyTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                                // update Bot status
                                await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);

                                // update Task executorId
                                await proxyTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", Context.UserIdentifier!);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            else
            {
                // send Task to bot
                await Clients.Caller.SendAsync($"do_new_proxycombiner_{task.InternalType.ToString().ToLower()}_task", task);
            }

            //await ManagementHubContext.Clients.All.SendAsync("proxy_combiner_task_status_changed", task);
        }
        public async Task proxy_combiner_defaultcombine_task_status_changed(ParseTask task)
        {
            await ManagementHubContext.Clients.All.SendAsync("proxy_combiner_task_status_changed", task);
        }
        public async Task proxy_combiner_errors_log(SiteParseBalancer error_site)
        {
            try
            {
                //++count of parsed site to error site in parsebalancercollection
                string command = "$set";

                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { nameof(error_site.CountParseSite), (error_site.CountParseSite += 25) }
                };

                await siteParseBalancersService.UpdateAsync(error_site.Id!, data, command);
                
                // add to proxytaskerrorlog
                error_site.Id = ObjectId.GenerateNewId().ToString();
                await proxyTasksErorrsLog.CreateAsync(error_site);
            }
            catch (Exception e)
            {
                Console.WriteLine("Перехвачена ошибка:\n" + e.Message);
            }
        }

        #endregion

        #region CAPTCHA SERVICE

        public async Task solve_captcha_recaptchav2(ReCaptchaV2 captcha)
        {
            try
            {
                await CaptchaGuruRu.CreateResolveCaptchaTask(captcha);
            }
            catch(Exception e)
            {
                captcha.status = CaptchaStatus.Error;
                captcha.statusMessage = e.Message;
            }
            finally
            {
                await Clients.Caller.SendAsync("changed_status_solve_captcha_recaptchav2", captcha);
            }
        }

        public async Task check_status_solve_captcha_recaptchav2(ReCaptchaV2 captcha)
        {
            try
            {
                await CaptchaGuruRu.CheckStatusResolveCaptchaTask(captcha);
            }
            catch (Exception e)
            {
                captcha.status = CaptchaStatus.Error;
                captcha.statusMessage = e.Message;
            }
            finally
            {
                await Clients.Caller.SendAsync("changed_status_solve_captcha_recaptchav2", captcha);
            }
        }

        public async Task solve_captcha_cloudflare_turnstile(CloudflareTurnstile captcha)
        {
            try
            {
                await CaptchaGuruRu.CreateResolveCaptchaTask(captcha);
            }
            catch (Exception e)
            {
                captcha.status = CaptchaStatus.Error;
                captcha.statusMessage = e.Message;
            }
            finally
            {
                await Clients.Caller.SendAsync("changed_status_solve_captcha_cloudflare_turnstile", captcha);
            }
        }

        public async Task check_status_solve_captcha_cloudflare_turnstile(CloudflareTurnstile captcha)
        {
            try
            {
                await CaptchaGuruRu.CheckStatusResolveCaptchaTask(captcha);
            }
            catch (Exception e)
            {
                captcha.status = CaptchaStatus.Error;
                captcha.statusMessage = e.Message;
            }
            finally
            {
                await Clients.Caller.SendAsync("changed_status_solve_captcha_cloudflare_turnstile", captcha);
            }
        }

        #endregion

        #region PLATFORM SOCPUBLICCOM

        public async Task set_task_status_to_waiting_for_check_account_history_proxy(string task_id)
        {
            // update task status
            await earnSiteTasksService.UpdateAsync(task_id, "Status", (int)TaskStatus.WaitingForCheckAccountHistoryProxy);
        }

        public async Task set_task_status_to_waiting_for_new_proxy(string task_id)
        {
            // update task status
            await earnSiteTasksService.UpdateAsync(task_id, "Status", (int)TaskStatus.WaitingForNewProxy);
        }

        public async Task set_task_status_to_waiting_for_captcha(string task_id)
        {
            // update task status
            await earnSiteTasksService.UpdateAsync(task_id, "Status", (int)TaskStatus.WaitingForCaptcha);
        }

        public async Task set_task_status_to_executing(string task_id)
        {
            // update task status
            await earnSiteTasksService.UpdateAsync(task_id, "Status", (int)TaskStatus.Executing);
        }
        public async Task set_withdrawal_task_status_to_started_by_slave(string task_id)
        {
            // update task status
            await earnSiteTasksService.UpdateAsync(task_id, "InternalStatus", (int)WithdrawalInternalStatus.StartedBySlave);
        }


        public async Task<bool> check_useragent_on_unique(string? useragent)
        {
            bool unique = false;

            if(useragent != null)
            {
                try
                {
                    unique = await socpublicAccountsService.CheckUserAgentOnUnique(useragent);
                }
                catch { }
            }

            return unique;
        }

        public async Task<PlatformInternalAccountTaskModel?> get_slave_internal_account_task(string acc_slave_login)
        {
            PlatformInternalAccountTaskModel? slave_platform_internal_task = null;

            try
            {
                slave_platform_internal_task = await platformInternalAccountTaskService.GetBySlaveAccountLoginAsync(acc_slave_login);
            }
            catch { }

            return slave_platform_internal_task;
        }
        //public async Task<bool> update_slave_internal_account_task(string acc_slave_login, string internal_task_number_id)
        //{
        //    bool update = false;

        //    try
        //    {
        //        await platformInternalAccountTaskService.UpdateBySlaveAccLoginAsync(acc_slave_login, "InternalNumberId", internal_task_number_id);
        //        update = true;
        //    }
        //    catch { }

        //    return update;
        //}
        public async Task update_slave_internal_account_task(string acc_slave_login, string internal_task_number_id)
        {
            try
            {
                await platformInternalAccountTaskService.UpdateBySlaveAccLoginAsync(acc_slave_login, "InternalNumberId", internal_task_number_id);
            }
            catch { }
        }
        public async Task<WithdrawMoneyGroupSelectiveTaskWithAuth?> get_new_withdraw_task_to_execute_by_main(string acc_main_id, string acc_main_login)
        {
            WithdrawMoneyGroupSelectiveTaskWithAuth? slave_task = null;

            try
            {
                slave_task = await earnSiteTasksService.GetOneForWithdrawByMain(acc_main_id, acc_main_login, Context.UserIdentifier!);

                // set task status to execute
                if (slave_task != null)
                {
                    // update task status
                    await earnSiteTasksService.UpdateAsync(slave_task.Id!, "Status", (int)TaskStatus.Executing); 
                }
            }
            catch { }

            return slave_task;
        }
        public async Task<bool> check_all_withdrawal_tasks_are_completed(string acc_main_id, string acc_main_login)
        {
            bool all_withdrawal_tasks_are_completed = false;

            try
            {
                all_withdrawal_tasks_are_completed = await earnSiteTasksService.CheckIfNoMoreTasksExistForWithdrawByMain(acc_main_id, acc_main_login, Context.UserIdentifier!);
            }
            catch { }

            return all_withdrawal_tasks_are_completed;
        }


        public async Task platform_socpubliccom_task_status_changed(GroupSelectiveTaskWithAuth doned_task)
        {
            doned_task.Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Done;

            // check run tasks settings
            //var tasksSettings = await runTasksSettingsService.GetAsync();
            //RunTasksSettings? runTasksSettings = null;
            //if (tasksSettings.Count == 1)
            //{
            //    runTasksSettings = tasksSettings[0];
            //}

            //save results and give new task to bot
            if (doned_task.Account!.MoneyMainBalance == null)
            {
                doned_task.Account.MoneyMainBalance = 0;
            }

            //doned_task.Account.MoneyMainBalance += doned_task.taskVisitWithoutTimer!.moneyEarned;
            //if (doned_task.Account!.History![doned_task.Account!.History.Count - 1].MoneyMainBalancePlus != null)
            //{
            //    doned_task.Account.MoneyMainBalance += doned_task.Account!.History![doned_task.Account!.History.Count - 1].MoneyMainBalancePlus;
            //}

            await socpublicAccountsService.ReplaceAsync(doned_task.Account!.Id!, doned_task.Account!);

            //auto relaunch task with error
            if (doned_task.ResultStatus == TaskResultStatus.Error && doned_task.ErrorStatus == TaskErrorStatus.ConnectionError)
            {
                if (doned_task.ErrorTaskCount == null)
                {
                    doned_task.ErrorTaskCount = 1;
                }
                else
                {
                    doned_task.ErrorTaskCount += 1;
                }

                if (doned_task.ErrorTaskCount <= 5 && doned_task.Account.IsAlive.HasValue && doned_task.Account.IsAlive.Value)
                {
                    // relaunch here
                    SocpublicComTaskSubtype[] socpublicComTaskSubtypes = new SocpublicComTaskSubtype[]
                    {
                        SocpublicComTaskSubtype.InstallAvatar,
                        SocpublicComTaskSubtype.FillAnketaMainInfo,
                        SocpublicComTaskSubtype.PassRulesKnowledgeTest,
                        SocpublicComTaskSubtype.CheckAndGetFreeGiftBoxes,
                        SocpublicComTaskSubtype.TaskVisitWithoutTimer,
                        SocpublicComTaskSubtype.UpdatingTheCurrentBalance,
                        SocpublicComTaskSubtype.CheckCareerLadder
                    };
                    //SocpublicComTaskSubtype[] socpublicComTaskSubtypes = new SocpublicComTaskSubtype[]
                    //{
                    //    //SocpublicComTaskSubtype.InstallAvatar,
                    //    //SocpublicComTaskSubtype.CheckOnNeedToConfirmAccountEmail,
                    //    //SocpublicComTaskSubtype.ConfirmRegisterAccountByLinkFromEmail,
                    //    SocpublicComTaskSubtype.FillAnketaMainInfo,
                    //    SocpublicComTaskSubtype.PassRulesKnowledgeTest,
                    //    SocpublicComTaskSubtype.CheckAndGetFreeGiftBoxes,
                    //    SocpublicComTaskSubtype.TaskVisitWithoutTimer,
                    //    SocpublicComTaskSubtype.UpdatingTheCurrentBalance,
                    //    SocpublicComTaskSubtype.CheckCareerLadder
                    //};

                    GroupSelectiveTaskWithAuth task = new GroupSelectiveTaskWithAuth("1", TaskFrom.Management, SocpublicComUrl.http, doned_task!.Account!.Id!, doned_task.Account, socpublicComTaskSubtypes)
                    {
                        Id = doned_task.Id,
                        QueuePosition = 0,
                        Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
                        ResultStatus = TaskResultStatus.Unknown,
                        ActionAfterFinish = TaskActionAfterFinish.Renew,
                        taskVisitWithoutTimer = new TaskVisitWithoutTimer(),
                        checkCareerLadder = new CheckCareerLadder(),
                        ErrorTaskCount = doned_task.ErrorTaskCount
                    };

                    //// create new task for this account
                    //await earnSiteTasksService.CreateAsync(task);

                    //// remove doned task with error for this account
                    //await earnSiteTasksService.DeleteAsync(doned_task.Id!);

                    // replace with new task for this account
                    await earnSiteTasksService.ReplaceAsync(doned_task.Id!, task);
                }
                else
                {
                    await earnSiteTasksService.ReplaceAsync(doned_task.Id!, doned_task!);
                }
            }
            else
            {
                await earnSiteTasksService.ReplaceAsync(doned_task.Id!, doned_task!);
            }

            // dispose bot task
            await Clients.Caller.SendAsync("dispose_platform_socpubliccom_and_set_new_token", "token herokin)");

            var bot = await clientsBotService.GetAsync(Context.UserIdentifier!);

            if (bot != null && bot.Status != ClientStatus.Stopped)
            {
                await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.Free);

                // get task
                try
                {
                    //List<dynamic> tasks = await earnSiteTasksService.GetAsync();
                    //dynamic? task = tasks!.Where(t => t.Type == CommonModels.ProjectTask.ProjectTaskEnums.TaskType.EarningSiteWork && t.Status == CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created).FirstOrDefault();

                    //GroupSelectiveTaskWithAuth normalTask = task!;
                    GroupSelectiveTaskWithAuth? normalTask = await earnSiteTasksService.GetOneCreatedAsync();

                    if (normalTask != null)
                    {
                        // send Task to bot
                        await Clients.Caller.SendAsync("do_new_platform_socpubliccom_task", normalTask);

                        // update task status
                        await earnSiteTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                        // update Bot status
                        await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);

                        // update Task executorId
                        await earnSiteTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", Context.UserIdentifier!);
                    }
                    else
                    {
                        // set run tasks settings
                        var tasksSettings = await runTasksSettingsService.GetAsync();
                        if (tasksSettings.Count == 1)
                        {
                            var taskSett = tasksSettings[0];
                            taskSett.TasksIsRunning = false;
                            taskSett.CurrentLaunchedTaskType = CommonModels.ProjectTask.LaunchTaskType.EarningMoney;

                            await runTasksSettingsService.ReplaceAsync(taskSett.Id!, taskSett);
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                //await ManagementHubContext.Clients.All.SendAsync("platform_socpubliccom_task_status_changed", task); 
            }
        }
        public async Task platform_socpubliccom_autoreg_task_status_changed(SocpublicComAutoregTask doned_task)
        {
            //save results and give new task to bot

            doned_task.Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Done;

            if(doned_task.Account != null)
            {
                await socpublicAccountsService.CreateAsync(doned_task.Account!);

                string[] emailsBase = await File.ReadAllLinesAsync(@"currentlistaccounts.txt");
                List<string> emails = emailsBase.ToList();
                emails.RemoveAll(e => e.Contains(doned_task.Email.Address));
                await File.WriteAllLinesAsync(@"currentlistaccounts.txt", emails);
            }

            await earnSiteTasksService.ReplaceAsync(doned_task.Id!, doned_task!);

            // dispose bot task
            await Clients.Caller.SendAsync("dispose_platform_socpubliccom_and_set_new_token", "token herokin)");

            var bot = await clientsBotService.GetAsync(Context.UserIdentifier!);

            if (bot != null && bot.Status != ClientStatus.Stopped)
            {
                await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.Free);

                // get task
                //List<dynamic> tasks = await earnSiteTasksService.GetAsync();
                //dynamic? task = tasks.Where(t => t.InternalType == CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums.SocpublicComTaskType.AutoregTask && t.Status == CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created).FirstOrDefault();

                //SocpublicComAutoregTask normalTask = task!;
                SocpublicComAutoregTask? normalTask = await earnSiteTasksService.GetOneCreatedAsync();

                if (normalTask != null)
                {
                    // send Task to bot
                    await Clients.Caller.SendAsync("do_new_platform_socpubliccom_autoreg_task", normalTask);

                    // update task status
                    await earnSiteTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                    // update Bot status
                    await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);

                    // update Task executorId
                    await earnSiteTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", Context.UserIdentifier!);
                }
                else
                {
                    // set run tasks settings
                    var tasksSettings = await runTasksSettingsService.GetAsync();
                    if (tasksSettings.Count == 1)
                    {
                        var taskSett = tasksSettings[0];
                        taskSett.TasksIsRunning = false;
                        taskSett.CurrentLaunchedTaskType = CommonModels.ProjectTask.LaunchTaskType.AutoregAccounts;

                        await runTasksSettingsService.ReplaceAsync(taskSett.Id!, taskSett);
                    }
                }

                //await ManagementHubContext.Clients.All.SendAsync("platform_socpubliccom_task_status_changed", task); 
            }
        }
        public async Task platform_socpubliccom_withdrawal_task_status_changed(WithdrawMoneyGroupSelectiveTaskWithAuth doned_task)
        {
            if(doned_task.Account!.IsMain.HasValue && !doned_task.Account.IsMain.Value)
            {
                if(doned_task.InternalStatus == WithdrawalInternalStatus.CompletedSuccessfullyByMain || doned_task.InternalStatus == WithdrawalInternalStatus.CompletedWithErrorByMain)
                {
                    doned_task.Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Done;
                }
                else if(doned_task.InternalStatus == WithdrawalInternalStatus.CompletedWithErrorBySlave)
                {
                    doned_task.Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Done;
                }
                else if(doned_task.InternalStatus == WithdrawalInternalStatus.CompletedSuccessfullBySlave)
                {
                    doned_task.Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Executing;
                }
            }
            else if (doned_task.Account!.IsMain.HasValue && doned_task.Account.IsMain.Value)
            {
                doned_task.Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Done;
            }

            await socpublicAccountsService.ReplaceAsync(doned_task.Account!.Id!, doned_task.Account!);

            await earnSiteTasksService.ReplaceAsync(doned_task.Id!, doned_task!);

            var bot = await clientsBotService.GetAsync(Context.UserIdentifier!);

            if (bot != null && bot.Status != ClientStatus.Stopped)
            {
                // выдаем новое задание боту только в случае если это бот со слейвом на борту, бот с мейном же сам будет запрашивать задания для 2го этапа вывода денег
                if (doned_task.Account!.IsMain.HasValue && !doned_task.Account.IsMain.Value && (doned_task.InternalStatus == WithdrawalInternalStatus.StartedBySlave || doned_task.InternalStatus == WithdrawalInternalStatus.CompletedSuccessfullBySlave || doned_task.InternalStatus == WithdrawalInternalStatus.CompletedWithErrorBySlave))
                {
                    // dispose bot task
                    await Clients.Caller.SendAsync("dispose_platform_socpubliccom_and_set_new_token", "token herokin)");

                    await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.Free);

                    // get task
                    try
                    {
                        //List<dynamic> tasks = await earnSiteTasksService.GetAsync();
                        //dynamic? task = tasks!.Where(t => t.Type == CommonModels.ProjectTask.ProjectTaskEnums.TaskType.EarningSiteWork && t.Status == CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created).FirstOrDefault();

                        //GroupSelectiveTaskWithAuth normalTask = task!;
                        WithdrawMoneyGroupSelectiveTaskWithAuth? normalTask = await earnSiteTasksService.GetOneCreatedWirthdrawForSlaveAsync();

                        if (normalTask != null)
                        {
                            // send Task to bot
                            await Clients.Caller.SendAsync("do_new_platform_socpubliccom_withdrawal_task", normalTask);

                            // update task status
                            await earnSiteTasksService.UpdateAsync(normalTask.Id!, nameof(normalTask.Status), (int)TaskStatus.Executing);

                            // update Bot status
                            await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.AtWork);

                            // update Task executorId
                            await earnSiteTasksService.UpdateAsync(normalTask.Id!, "ExecutorId", Context.UserIdentifier!);
                        }
                        else
                        {
                            // set run tasks settings
                            var tasksSettings = await runTasksSettingsService.GetAsync();
                            if (tasksSettings.Count == 1)
                            {
                                var taskSett = tasksSettings[0];
                                taskSett.TasksIsRunning = false;
                                taskSett.CurrentLaunchedTaskType = CommonModels.ProjectTask.LaunchTaskType.WithdrawingMoney;

                                await runTasksSettingsService.ReplaceAsync(taskSett.Id!, taskSett);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if(doned_task.Account!.IsMain.HasValue && doned_task.Account.IsMain.Value)
                {
                    // dispose bot task
                    await Clients.Caller.SendAsync("dispose_platform_socpubliccom_and_set_new_token", "token herokin)");

                    await clientsBotService.UpdateAsync(Context.UserIdentifier!, "Status", (int)ClientStatus.Free);
                }
            }
        }

        public async Task addClientTo_SOCPUBLIC_DOT_COM_Group()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "socpublic.com");
        }

        #endregion









        /*public async Task messageFromUnauthorizedClient(string message)
        {
            Console.Write(message);
        }

        public async Task messageFromAuthorizedClient(string message, Client client)
        {
            Console.Write($"\n[{client.Roles![0]}@{client.ID}]: " + message);
        }*/

        /*        [Authorize(Roles = $"{nameof(Role.Bot)}, {nameof(Role.Manager)}")]
                public async Task addClientToHubGroup(string groupName)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                }*/

        /*[HubAuthorizeIP]
        [Authorize(Roles = $"{nameof(Role.Bot)}, {nameof(Role.Manager)}")]
        public async Task test(Client client)
        {
            // этот список создается так: мы получаем из аргументов метода или из базы данных( в зависимости от от вызваного метода ) список обьектов класса "Proxy",
            // далее этот список мы "переукомплектовуем" в новый список обьектов класса "CheckedProxy" в котором добавляются некоторые поля, по типу "requestUrl", "requestTimeOut", "PlatformType"
            var list = new List<CheckedProxy>()
            {
                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "172.67.181.138",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "172.67.181.138",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.24.103.226",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.22.223.44",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "64.225.8.121",
                    Port = "9989"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.206.106.192",
                    Port = "8123"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "89.43.31.134",
                    Port = "3128"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "162.55.63.236",
                    Port = "8080"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.24.43.214",
                    Port = "8123"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.111.54.16",
                    Port = "8123"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "54.37.137.112",
                    Port = "8118"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "157.245.27.9",   ////////////////
                    Port = "3128"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "91.107.247.115",  //////////////
                    Port = "4000"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "24.199.106.243",
                    Port = "3128"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "192.252.209.155",
                    Port = "14455"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "98.162.25.16",
                    Port = "4145"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "185.87.121.5",
                    Port = "8975"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "5.182.26.129",   ///////////////////
                    Port = "4145"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "104.248.29.72",
                    Port = "2333"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "89.208.105.58",
                    Port = "1488"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "193.141.65.48",  //////////////////////
                    Port = "8975"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "98.162.25.23",
                    Port = "4145"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "104.37.135.145",
                    Port = "4145"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "63.53.204.178",
                    Port = "9813"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "139.59.1.14",
                    Port = "1080"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.23.104.31",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "185.238.228.202",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.22.223.135",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.34.28.135",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "45.8.105.62",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "45.8.105.187",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "185.162.229.76",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.23.104.45",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.23.104.180",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "45.8.105.229",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "45.12.31.177",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "185.162.228.109",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.13.32.110",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "185.238.228.178",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "185.238.228.37",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "141.101.120.37",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "45.131.5.243",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "45.131.5.85",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "172.67.181.208",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.23.106.81",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.24.103.38",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.23.103.126",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "45.131.4.34",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "172.64.196.183",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "172.67.181.138",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "45.8.104.253",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.30.189.242",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.30.189.19",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.23.104.90",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "141.193.213.179",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "185.238.228.12",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "23.227.39.125",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "172.67.254.145",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.28.9.83",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.22.223.139",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "193.141.65.48",  //////////////////////
                    Port = "8975"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "5.182.26.129",   ///////////////////
                    Port = "4145"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.22.223.139",
                    Port = "80"
                }),

                #region HTTPS
                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "31.44.7.206",
                    Port = "443"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "129.151.173.15",
                    Port = "8080"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.111.54.16",
                    Port = "8123"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "185.166.25.98",
                    Port = "8080"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "89.43.31.134",
                    Port = "3128"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "24.199.106.239",
                    Port = "3128"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "88.99.234.110",
                    Port = "2021"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "88.99.131.6",
                    Port = "8118"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "195.201.226.34",
                    Port = "4000"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "95.56.254.139",
                    Port = "3128"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.206.106.192",
                    Port = "8123"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.206.106.192",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.24.43.214",
                    Port = "8123"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.210.113.32",
                    Port = "8123"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.210.113.32",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.24.43.214",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.205.61.143",
                    Port = "8123"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.205.61.143",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "20.111.54.16",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "91.107.247.138",
                    Port = "4000"
                }),
                #endregion
                #region HTTP
                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "87.247.185.75",
                    Port = "3128"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "50.204.190.234",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.30.188.218",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "203.28.8.47",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "45.12.31.61",
                    Port = "80"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "45.14.174.8",
                    Port = "80"
                }),
                #endregion
                #region SOCKS5
                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "72.221.232.155",
                    Port = "4145"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "72.221.232.152",
                    Port = "4145"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "98.188.47.150",
                    Port = "4145"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "68.1.210.163",
                    Port = "4145"
                }),

                new CheckedProxy(new CommonModels.Proxy()
                {
                    Address = "68.71.249.153",
                    Port = "48606"
                }),
                #endregion

            };

            var requirements = new CheckedProxyRequirements(true, 5000, "http://socpublic.com", "https://socpublic.com", CommonModels.PlatformType.Type.SocpublicDotCom, CheckedProxyRequirements.requiredProxyProtocol.all);

            var data = new ProxyCheckTypeList(list, requirements);

            IProjectTask<ProxyCheckTypeList> projectTask = new ProxyCheckTask<ProxyCheckTypeList>(Context.UserIdentifier!, TaskSubtype.ClientTaskType5, data);

            //await Clients.Group("FreeBot").SendAsync("test", projectTask);
            await Clients.Others.SendAsync("test", projectTask);
        }*/

        //[Authorize(Roles = $"{nameof(BotRole.PlatformWorkBot)}, {nameof(BotRole.BotManager)}")]
        //public async Task test2()
        //{
        //    await Clients.Others.SendAsync("test2");
        //}

        //[Authorize(Roles = "Bot")]
        //public async Task getNewAccountTask(Client client)
        //{
        //    // send new task to bot
        //    // get account from db
        //    await Clients.Caller.SendAsync("");
        //}

        //public async Task sendTaskFromAdmin()
        //{
        //    await Clients.All.SendAsync("StartWork");
        //}
    }
}
