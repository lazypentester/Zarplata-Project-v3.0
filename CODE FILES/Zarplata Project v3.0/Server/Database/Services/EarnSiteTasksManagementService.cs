using CommonModels.Client.Models.SearchBotsModels;
using CommonModels.Client.Models;
using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using CommonModels.ProjectTask.EarningSite.SearchTasksModels;
using CommonModels.Client.Models.DeleteBotsModels;
using CommonModels.ProjectTask.EarningSite.DeleteTasksModels;
using Newtonsoft.Json;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using static CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.SelectiveTaskWithAuth;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using static CommonModels.ProjectTask.Platform.EarningSiteTaskUrls;
using MongoDB.Bson;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth.SelectiveTask.TaskVisitWithoutTimer;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth.SelectiveTask.CheckCareerLadder;
using static CommonModels.Client.Client;
using TaskStatus = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using CommonModels.EmailModels;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels;
using System.Linq;
using CommonModels.ProjectTask;
using System.Threading.Tasks;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney;
using System.Security.Principal;
using AngleSharp.Text;
using CommonModels.ProjectTask.EarningSite.SocpublicCom.Models.WithdrawalOfMoneyModels;

namespace Server.Database.Services
{
    public class EarnSiteTasksManagementService
    {
        private readonly EarnSiteTasksService earnSiteTasksService;
        private readonly SocpublicAccountsService socpublicAccountsService;
        private readonly ClientsService clientsBotService;
        private readonly ProxyTasksService proxyTasksService;
        private readonly IHubContext<ManagementHub> ManagementHubContext;
        private readonly IHubContext<ClientHub> ClientHubContext;
        private readonly RunTasksSettingsService runTasksSettingsService;
        private readonly PlatformInternalAccountTaskService platformInternalAccountTaskService;
        private readonly PlatformInternalAccountTaskService platformInternalAccountTasksCollection;

        public EarnSiteTasksManagementService(EarnSiteTasksService earnSiteTasksService, IHubContext<ManagementHub> managementHubContext, SocpublicAccountsService socpublicAccountsService, ClientsService clientsBotService, IHubContext<ClientHub> clientHubContext, ProxyTasksService proxyTasksService, RunTasksSettingsService runTasksSettingsService, PlatformInternalAccountTaskService platformInternalAccountTaskService, PlatformInternalAccountTaskService platformInternalAccountTasksCollection)
        {
            this.earnSiteTasksService = earnSiteTasksService;
            ManagementHubContext = managementHubContext;
            this.socpublicAccountsService = socpublicAccountsService;
            this.clientsBotService = clientsBotService;
            ClientHubContext = clientHubContext;
            this.proxyTasksService = proxyTasksService;
            this.runTasksSettingsService = runTasksSettingsService;
            this.platformInternalAccountTaskService = platformInternalAccountTaskService;
            this.platformInternalAccountTasksCollection = platformInternalAccountTasksCollection;
        }

        public async Task<FoundedTasks?> GetTasksCollection(FindTasks findTasks)
        {
            FoundedTasks? tasksFinalCollection = null;
            List<dynamic>? tasks = new List<dynamic>();

            // find collection by filters
            try
            {
                tasks.AddRange(await earnSiteTasksService.GetWithFiltersAndSortByCreateDateAsync(
                        findTasks.FindFilterTasks.FindFiltersInTypeInt,
                        findTasks.FindFilterTasks.FindFiltersInTypeString,
                        findTasks.FindFilterTasks.FindSearchKeyword,
                        findTasks.FindFilterTasks.FindSearchKeywordDateTime,
                        findTasks.FindSortTasks
                        ));
            }
            catch(Exception ex)
            {

            }

            // разбиваем коллекцию на страницы и берем текущую выбраную страницу
            if (tasks != null)
            {
                try
                {
                    double pageCount = (double)tasks.Count / (double)findTasks.ResultsPerPage;
                    double pageCountRounded = Math.Round(pageCount, 0, MidpointRounding.ToPositiveInfinity);
                    int firstElementNumOnSelectedPage = 0;
                    int lastElementNumOnSelectedPage = 0;
                    int allTasksCount = tasks.Count;

                    if (findTasks.SelectedPageNumber > (int)pageCountRounded)
                    {
                        findTasks.SelectedPageNumber = (int)pageCountRounded;
                    }

                    // отсеиваем лишних задания, оставляем только одну страницу
                    List<dynamic> onePageTasks = tasks;
                    if ((int)pageCountRounded > 1)
                    {
                        try
                        {
                            int countElementsOnLastPageOfAllPages = tasks.Count - (((int)pageCountRounded - 1) * findTasks.ResultsPerPage);

                            if (findTasks.SelectedPageNumber == 1)
                            {
                                firstElementNumOnSelectedPage = 1;
                                lastElementNumOnSelectedPage = findTasks.ResultsPerPage;

                                onePageTasks.RemoveRange(findTasks.ResultsPerPage, (tasks.Count - findTasks.ResultsPerPage));
                            }
                            else if (findTasks.SelectedPageNumber == (pageCountRounded))
                            {
                                firstElementNumOnSelectedPage = tasks.Count - countElementsOnLastPageOfAllPages + 1;
                                lastElementNumOnSelectedPage = tasks.Count;

                                onePageTasks.RemoveRange(0, ((findTasks.SelectedPageNumber - 1) * findTasks.ResultsPerPage));
                            }
                            else
                            {
                                firstElementNumOnSelectedPage = ((findTasks.SelectedPageNumber - 1) * findTasks.ResultsPerPage) + 1;
                                lastElementNumOnSelectedPage = findTasks.SelectedPageNumber * findTasks.ResultsPerPage;

                                onePageTasks.RemoveRange((findTasks.SelectedPageNumber * findTasks.ResultsPerPage), (tasks.Count - (findTasks.SelectedPageNumber * findTasks.ResultsPerPage)));
                                onePageTasks.RemoveRange(0, ((findTasks.SelectedPageNumber * findTasks.ResultsPerPage) - findTasks.ResultsPerPage));
                            }
                        }
                        catch
                        {
                            onePageTasks = new List<dynamic>();
                        }
                    }
                    else
                    {
                        firstElementNumOnSelectedPage = 1;
                        lastElementNumOnSelectedPage = tasks.Count;
                    }

                    tasksFinalCollection = new FoundedTasks(
                        onePageTasks,
                        findTasks.SelectedPageNumber,
                        allTasksCount,
                        (int)pageCountRounded,
                        firstElementNumOnSelectedPage,
                        lastElementNumOnSelectedPage,
                        "Задания успешно найдены",
                        true);
                }
                catch { }
            }

            return tasksFinalCollection;
        }
        


        public async Task<bool> CreateMassTaskSocpublicAutoreg()
        {
            // check run tasks settings
            var tasksSettings = await runTasksSettingsService.GetAsync();
            RunTasksSettings? runTasksSettings = null;
            if (tasksSettings.Count == 1)
            {
                runTasksSettings = tasksSettings[0];

                if (runTasksSettings.TasksIsRunning.HasValue && runTasksSettings.TasksIsRunning.Value)
                {
                    return false;
                }
                else if (!runTasksSettings.TasksIsRunning.HasValue)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            // сюда акки почт
            //string[] emailsBase = await File.ReadAllLinesAsync(@"C:\Users\glebg\Desktop\Zarplata Project v3.0\зареганные авторегом акки (1к купленных почт)\currentlistaccounts.txt");
            string[] emailsBase = await File.ReadAllLinesAsync(@"currentlistaccounts.txt");

            bool result = false;

            try
            {
                //clear proxy task queue
                await proxyTasksService.DeleteAsync();

                // clear earn task queue
                await earnSiteTasksService.DeleteAsync();

                List<SocpublicComAutoregTask> tasks = new List<SocpublicComAutoregTask>();

                for (int i = 0; i < emailsBase.Length; i++)
                {
                    var email = new Email(emailsBase[i].Split(':')[0], emailsBase[i].Split(':')[1], new CommonModels.Email.Settings.ConnectSettings("imap.firstmail.ltd", 993, true));

                    SocpublicComAutoregTask task = new SocpublicComAutoregTask("1", TaskFrom.Management, TaskStatus.Created, SocpublicComUrl.http, email, TaskActionAfterFinish.Remove)
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        QueuePosition = i,
                        Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
                        ResultStatus = TaskResultStatus.Unknown,
                        ActionAfterFinish = TaskActionAfterFinish.Renew
                    };

                    tasks.Add(task);
                }

                // create tasks
                await earnSiteTasksService.CreateAsync(tasks);

                // give tasks to bots
                List<ModelClient> bots = await clientsBotService.GetAsync();

                if (bots.Count == 0)
                    return false;

                List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.EarningSiteBot).ToList();

                if (freeBots.Count == 0)
                    return false;

                for (int i = 0; i < freeBots.Count && i < tasks.Count; i++)
                {
                    // send Task to bot
                    await ClientHubContext.Clients.User(freeBots[i].ID!).SendAsync("do_new_platform_socpubliccom_autoreg_task", tasks[i]);

                    // update task status
                    await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "Status", (int)TaskStatus.Started);

                    // update Bot status
                    await clientsBotService.UpdateAsync(freeBots[i].ID!, "Status", (int)ClientStatus.AtWork);

                    // update Task executorId
                    await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "ExecutorId", freeBots[i].ID!);
                }

                // set run tasks settings
                runTasksSettings.TasksIsRunning = true;
                runTasksSettings.CurrentLaunchedTaskType = CommonModels.ProjectTask.LaunchTaskType.AutoregAccounts;
                await runTasksSettingsService.ReplaceAsync(runTasksSettings.Id!, runTasksSettings);

                result = true;
            }
            catch (Exception e) { }

            return result;
        }

        public async Task<bool> CreateMassTaskSocpublicEarn()
        {
            // check run tasks settings
            var tasksSettings = await runTasksSettingsService.GetAsync();
            RunTasksSettings? runTasksSettings = null;
            if (tasksSettings.Count == 1)
            {
                runTasksSettings = tasksSettings[0];

                if (runTasksSettings.TasksIsRunning.HasValue && runTasksSettings.TasksIsRunning.Value)
                {
                    return false;
                }
                else if (!runTasksSettings.TasksIsRunning.HasValue)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            bool result = false;

            // main work
            try
            {
                // clear tasks queues
                if (runTasksSettings.CurrentLaunchedTaskType != LaunchTaskType.EarningMoney)
                {
                    // clear earn task queue
                    await earnSiteTasksService.DeleteAsync();

                    //clear proxy task queue
                    await proxyTasksService.DeleteAsync();
                }
                else
                {
                    //clear proxy task queue
                    await proxyTasksService.DeleteAsync();
                }

                // with confirm email
                //SocpublicComTaskSubtype[] socpublicComTaskSubtypes = new SocpublicComTaskSubtype[]
                //{
                //     SocpublicComTaskSubtype.ConfirmRegisterAccountByLinkFromEmail,
                //     SocpublicComTaskSubtype.TaskVisitWithoutTimer
                //};

                //with check confirm email
                //SocpublicComTaskSubtype[] socpublicComTaskSubtypes = new SocpublicComTaskSubtype[]
                //{
                //    SocpublicComTaskSubtype.CheckOnNeedToConfirmAccountEmail,
                //    SocpublicComTaskSubtype.TaskVisitWithoutTimer
                //};


                SocpublicComTaskSubtype[] socpublicComTaskSubtypes = new SocpublicComTaskSubtype[]
                {
                    SocpublicComTaskSubtype.InstallAvatar,
                    //SocpublicComTaskSubtype.CheckOnNeedToConfirmAccountEmail,
                    //SocpublicComTaskSubtype.ConfirmRegisterAccountByLinkFromEmail,
                    SocpublicComTaskSubtype.FillAnketaMainInfo,
                    SocpublicComTaskSubtype.PassRulesKnowledgeTest,
                    SocpublicComTaskSubtype.CheckAndGetFreeGiftBoxes,
                    SocpublicComTaskSubtype.TaskVisitWithoutTimer,
                    SocpublicComTaskSubtype.UpdatingTheCurrentBalance,
                    SocpublicComTaskSubtype.CheckCareerLadder
                };

                List<dynamic> tasks_dynamic_collection = await earnSiteTasksService.GetAsync();

                List<Account> socpublicAccounts = new List<Account>();
                try
                {
                    socpublicAccounts = await socpublicAccountsService.GetAsync();
                    socpublicAccounts.RemoveAll(acc => acc.IsAlive != null && acc.IsAlive == false);

                    //remove after
                    //socpublicAccounts.RemoveAll(acc => acc.EmailIsConfirmed.HasValue && acc.EmailIsConfirmed.Value == false);
                }
                catch (Exception e) { }

                List<GroupSelectiveTaskWithAuth> tasks = new List<GroupSelectiveTaskWithAuth>();

                if (socpublicAccounts.Count == 0)
                    return false;

                if (tasks_dynamic_collection.Count > 0)
                {
                    // сделать как можно проще, достать задания которые нужно пересоздать, затем полностью очистить очередь заданий, и заново создать новую очередь.

                    // аккаунты для пересоздания для них заданий
                    List<string?> socpublicAccountsIdsToRecreateTasks = tasks_dynamic_collection.Where(t => t.ResultStatus != TaskResultStatus.Success).Select(t => (t.Account as Account)!.Id).ToList();

                    //remove after
                    //try
                    //{
                    //    string ids_text = "";
                    //    socpublicAccountsIdsToRecreateTasks.ForEach(id => ids_text += id + Environment.NewLine);

                    //    await File.WriteAllTextAsync($@"C:\Users\glebg\Desktop\socpublicAccountsIdsToRecreateTasks.txt", ids_text);
                    //}
                    //catch { }

                    foreach (var account in socpublicAccounts)
                    {
                        //Убрать мейны с поcещений чтобы их не забанили
                        if (account.IsMain.HasValue && account.IsMain.Value)
                        {
                            continue;
                        }

                        if(socpublicAccountsIdsToRecreateTasks.Contains(account.Id))
                        {
                            GroupSelectiveTaskWithAuth task = new GroupSelectiveTaskWithAuth("1", TaskFrom.Management, SocpublicComUrl.http, account.Id!, account, socpublicComTaskSubtypes)
                            {
                                Id = ObjectId.GenerateNewId().ToString(),
                                QueuePosition = 0,
                                Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
                                ResultStatus = TaskResultStatus.Unknown,
                                ActionAfterFinish = TaskActionAfterFinish.Renew,
                                taskVisitWithoutTimer = new TaskVisitWithoutTimer(),
                                checkCareerLadder = new CheckCareerLadder()
                            };

                            tasks.Add(task);
                        }
                    }

                    // sort tasks by account.DateTimeStart of last history session
                    try
                    {
                        tasks.Sort((x, y) => x.Account!.History![x.Account.History.Count - 1].DateTimeStart!.Value.CompareTo(y.Account!.History![y.Account.History.Count - 1].DateTimeStart!.Value));
                    }
                    catch (Exception)
                    {
                    }

                    // remove all task queue
                    await earnSiteTasksService.DeleteAsync();

                    // create tasks
                    await earnSiteTasksService.CreateAsync(tasks);

                    // give tasks to bots
                    List<ModelClient> bots = await clientsBotService.GetAsync();

                    if (bots.Count == 0)
                        return false;

                    List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.EarningSiteBot).ToList();

                    if (freeBots.Count == 0)
                        return false;

                    for (int i = 0; i < freeBots.Count && i < tasks.Count; i++)
                    {
                        // send Task to bot
                        await ClientHubContext.Clients.User(freeBots[i].ID!).SendAsync("do_new_platform_socpubliccom_task", tasks[i]);

                        // update task status
                        await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "Status", (int)TaskStatus.Started);

                        // update Bot status
                        await clientsBotService.UpdateAsync(freeBots[i].ID!, "Status", (int)ClientStatus.AtWork);

                        // update Task executorId
                        await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "ExecutorId", freeBots[i].ID!);
                    }

                    if(tasks.Count > 0)
                    {
                        // set run tasks settings
                        runTasksSettings.TasksIsRunning = true;
                        runTasksSettings.CurrentLaunchedTaskType = CommonModels.ProjectTask.LaunchTaskType.EarningMoney;

                        await runTasksSettingsService.ReplaceAsync(runTasksSettings.Id!, runTasksSettings);
                    }
                }
                else
                {
                    //remove after
                    //List<string> socpublicAccountsIdsToRecreateTasks = File.ReadAllLines(@"C:\Users\glebg\Desktop\socpublicAccountsIdsToRecreateTasks.txt").ToList();
                    //socpublicAccountsIdsToRecreateTasks.RemoveAll(ac => String.IsNullOrEmpty(ac));

                    //foreach (var account in socpublicAccounts)
                    //{
                    //    if (socpublicAccountsIdsToRecreateTasks.Contains(account.Id!))
                    //    {
                    //        GroupSelectiveTaskWithAuth task = new GroupSelectiveTaskWithAuth("1", TaskFrom.Management, SocpublicComUrl.http, account.Id!, account, socpublicComTaskSubtypes)
                    //        {
                    //            Id = ObjectId.GenerateNewId().ToString(),
                    //            QueuePosition = 0,
                    //            Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
                    //            ResultStatus = TaskResultStatus.Unknown,
                    //            ActionAfterFinish = TaskActionAfterFinish.Renew,
                    //            taskVisitWithoutTimer = new TaskVisitWithoutTimer(),
                    //            checkCareerLadder = new CheckCareerLadder()
                    //        };

                    //        tasks.Add(task);
                    //    }
                    //}

                    //remove after
                    //socpublicAccounts.RemoveAll(acc =>
                    //acc.Login != "xpomocoma777" &&
                    //acc.Login != "dashshendel" &&
                    //acc.Login != "ikuc365");
                    //socpublicAccounts.RemoveAll(acc =>
                    //acc.Login != "liiwlkxy" &&
                    //acc.Login != "oxgjwssx");
                    //socpublicAccounts.RemoveAll(acc =>
                    //acc.Login == "xpomocoma777" ||
                    //acc.Login == "dashshendel" ||
                    //acc.Login == "ikuc365");

                    for (int i = 0; i < socpublicAccounts.Count; i++)
                    {
                        //Убрать мейны с поcещений чтобы их не забанили
                        if (socpublicAccounts[i].IsMain.HasValue && socpublicAccounts[i].IsMain!.Value)
                        {
                            continue;
                        }

                        GroupSelectiveTaskWithAuth task = new GroupSelectiveTaskWithAuth("1", TaskFrom.Management, SocpublicComUrl.http, socpublicAccounts[i].Id!, socpublicAccounts[i], socpublicComTaskSubtypes)
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            QueuePosition = i,
                            Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
                            ResultStatus = TaskResultStatus.Unknown,
                            ActionAfterFinish = TaskActionAfterFinish.Renew,
                            taskVisitWithoutTimer = new TaskVisitWithoutTimer(),
                            checkCareerLadder = new CheckCareerLadder()
                        };

                        tasks.Add(task);
                    }

                    // sort tasks by account.DateTimeStart of last history session
                    try
                    {
                        tasks.Sort((x, y) => x.Account!.History![x.Account.History.Count - 1].DateTimeStart!.Value.CompareTo(y.Account!.History![y.Account.History.Count - 1].DateTimeStart!.Value));
                    }
                    catch (Exception)
                    {
                    }

                    // create tasks
                    await earnSiteTasksService.CreateAsync(tasks);

                    // give tasks to bots
                    List<ModelClient> bots = await clientsBotService.GetAsync();

                    if (bots.Count == 0)
                        return false;

                    List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.EarningSiteBot).ToList();

                    if (freeBots.Count == 0)
                        return false;

                    for (int i = 0; i < freeBots.Count && i < tasks.Count; i++)
                    {
                        // send Task to bot
                        await ClientHubContext.Clients.User(freeBots[i].ID!).SendAsync("do_new_platform_socpubliccom_task", tasks[i]);

                        // update task status
                        await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "Status", (int)TaskStatus.Started);

                        // update Bot status
                        await clientsBotService.UpdateAsync(freeBots[i].ID!, "Status", (int)ClientStatus.AtWork);

                        // update Task executorId
                        await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "ExecutorId", freeBots[i].ID!);
                    }

                    // set run tasks settings
                    runTasksSettings.TasksIsRunning = true;
                    runTasksSettings.CurrentLaunchedTaskType = CommonModels.ProjectTask.LaunchTaskType.EarningMoney;
                    await runTasksSettingsService.ReplaceAsync(runTasksSettings.Id!, runTasksSettings);

                    result = true;
                }
            }
            catch(Exception e) { }

            return result;
        }

        public async Task<bool> CreateMassTaskSocpublicGetMoney()
        {
            // check run tasks settings
            var tasksSettings = await runTasksSettingsService.GetAsync();
            RunTasksSettings? runTasksSettings = null;
            if (tasksSettings.Count == 1)
            {
                runTasksSettings = tasksSettings[0];

                if (runTasksSettings.TasksIsRunning.HasValue && runTasksSettings.TasksIsRunning.Value)
                {
                    return false;
                }
                else if (!runTasksSettings.TasksIsRunning.HasValue)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            bool result = false;


            //main work
            try
            {
                // clear tasks queues
                if (runTasksSettings.CurrentLaunchedTaskType != LaunchTaskType.WithdrawingMoney)
                {
                    // clear earn task queue
                    await earnSiteTasksService.DeleteAsync();

                    //clear proxy task queue
                    await proxyTasksService.DeleteAsync();
                }
                else
                {
                    //clear proxy task queue
                    await proxyTasksService.DeleteAsync();
                }

                SocpublicComTaskSubtype[] socpublicComTaskSubtypesForWithdrawal = new SocpublicComTaskSubtype[]
                {
                    SocpublicComTaskSubtype.WithdrawalOfMoney
                };

                List<dynamic> tasks_dynamic_collection = await earnSiteTasksService.GetAsync();
                List<Account> socpublicAccounts = new List<Account>();

                List<PlatformInternalAccountTaskModel> internalTasks = await platformInternalAccountTasksCollection.GetAsync();

                try
                {
                    socpublicAccounts = await socpublicAccountsService.GetAsync();


                    //remove after 
                    //socpublicAccounts.RemoveAll(acc =>
                    //acc.Login != "liiwlkxy" &&
                    //acc.Login != "yhvftnqd" &&
                    //acc.Login != "dejkhxdq" && 
                    //acc.Login != "fknxorsf" &&
                    //acc.Login != "hbpjcnts" &&
                    //acc.Login != "wkihcton" &&
                    //acc.Login != "txaveotv" &&
                    //acc.Login != "kgpgpwlm" &&
                    //acc.Login != "ornnngtv" &&
                    //acc.Login != "qttrfiqq" && 
                    //acc.Login != "dashshendel");


                    #region FILTERS

                    // отсееваем аккаунты которые забанены или "не живые"
                    socpublicAccounts.RemoveAll(acc =>
                    (acc.IsAlive != null && acc.IsAlive == false)
                    ||
                    (acc.IsAlive == null));

                    // отсееваем аккаунты которые еще не имеют возможности выводить деньги на рекламный баланс (по очкам если меньше 35 (вообще вроде меньше 25, но страхуемся))
                    socpublicAccounts.RemoveAll(acc =>
                    ((acc.CareerLadder != null && acc.CareerLadder.StatusPoints.HasValue && acc.CareerLadder.StatusPoints < 35)
                    ||
                    (acc.CareerLadder == null)
                    ||
                    (acc.CareerLadder.StatusPoints.HasValue == false))
                    &&
                    (acc.IsMain.HasValue && acc.IsMain.Value == false)
                    );

                    // фильтруем аккаунты которые зареганы меньше месяца(30 дней) на платформе
                    socpublicAccounts.RemoveAll(acc =>
                    ((acc.RegedDateTime != null && DateTime.UtcNow.Subtract(acc.RegedDateTime.Value).Days <= 30)
                    ||
                    (acc.RegedDateTime.HasValue == false))
                    &&
                    (acc.IsMain.HasValue && acc.IsMain.Value == false)
                    );

                    // другие фильтры по анкете
                    socpublicAccounts.RemoveAll(acc =>
                    ((acc.Anketa == null)
                    ||
                    (acc.Anketa.AvatarInstalled.HasValue == false)
                    ||
                    (acc.Anketa.AvatarInstalled.HasValue && acc.Anketa.AvatarInstalled.Value == false)
                    ||
                    (acc.Anketa.AnketaMainInfoIsFilled.HasValue == false)
                    ||
                    (acc.Anketa.AnketaMainInfoIsFilled.HasValue && acc.Anketa.AnketaMainInfoIsFilled.Value == false)
                    ||
                    (acc.Anketa.TestPassed.HasValue == false)
                    ||
                    (acc.Anketa.TestPassed.HasValue && acc.Anketa.TestPassed.Value == false))
                    &&
                    (acc.IsMain.HasValue && acc.IsMain.Value == false)
                    );

                    // отсееваем аккаунты у которых на балансах вместе взятых не хватает денег для запуска своего уже созданного в бд задания
                    //List<Account> accounts_to_remove_from_collection = new List<Account>();
                    //double task_pay_fee = 0.30; // комиссия задания на платформе, нужно учитывать сумму на балансе + комиссия
                    //foreach (var account in socpublicAccounts)
                    //{
                    //    if (account.IsMain.HasValue && account.IsMain.Value == true)
                    //    {
                    //        continue;
                    //    }

                    //    PlatformInternalAccountTaskModel? internalAccountTaskModel = await platformInternalAccountTaskService.GetPriceBySlaveAccountLoginAsync(account.Login!);
                    //    if (internalAccountTaskModel != null)
                    //    {
                    //        string? str_price = internalAccountTaskModel.price_user;

                    //        if (!String.IsNullOrEmpty(str_price))
                    //        {
                    //            double price = str_price.ToDouble();

                    //            if (((account.MoneyMainBalance ?? 0) + (account.MoneyAdvertisingBalance ?? 0)) < (price * task_pay_fee + price) && (account.MoneyMainBalance ?? 0) > 0) // это чтобы отсеять только те акки, которые с большей вероятностью запустили задание и оно было выполнено мейном, и оставить те акки у которых задание мб висит не выполненным еще
                    //            {
                    //                accounts_to_remove_from_collection.Add(account);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            accounts_to_remove_from_collection.Add(account);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        accounts_to_remove_from_collection.Add(account);
                    //    }
                    //}
                    //socpublicAccounts.RemoveAll(acc => accounts_to_remove_from_collection.Contains(acc)); 

                    #endregion
                }
                catch (Exception e) { }

                List<WithdrawMoneyGroupSelectiveTaskWithAuth> tasks = new List<WithdrawMoneyGroupSelectiveTaskWithAuth>();

                if (socpublicAccounts.Count == 0)
                    return false;

                if (tasks_dynamic_collection.Count > 0)
                {
                    #region создание заданий мейнов и резервирование ботов под мейнов и выдача им этих заданий
                    // создание заданий мейнов и резервирование ботов под мейнов и выдача им этих заданий

                    List<Account> socpublicMainAccounts = socpublicAccounts.Where(acc => acc.IsMain.HasValue && acc.IsMain.Value).ToList();
                    if (socpublicMainAccounts.Count == 0)
                        return false;

                    socpublicAccounts.RemoveAll(acc => acc.IsMain.HasValue && acc.IsMain.Value);

                    List<KeyValuePair<string, string>> mainAccountsAndTheirAttachedBots = new List<KeyValuePair<string, string>>();

                    // get free bots for reserve
                    List<ModelClient> bots = await clientsBotService.GetAsync();

                    if (bots.Count == 0 || bots.Count < socpublicMainAccounts.Count)
                        return false;

                    List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.EarningSiteBot).ToList();

                    if (freeBots.Count == 0 || freeBots.Count < socpublicMainAccounts.Count)
                        return false;

                    // remove all task queue
                    await earnSiteTasksService.DeleteAsync();

                    for (int i = 0; i < socpublicMainAccounts.Count; i++)
                    {
                        // attach main_acc with free bot
                        mainAccountsAndTheirAttachedBots.Add(new KeyValuePair<string, string>(socpublicMainAccounts[i].Login!, freeBots[i].ID!));

                        WithdrawMoneyGroupSelectiveTaskWithAuth task = new WithdrawMoneyGroupSelectiveTaskWithAuth("1", TaskFrom.Management, SocpublicComUrl.https, socpublicMainAccounts[i].Id!, socpublicMainAccounts[i], socpublicComTaskSubtypesForWithdrawal)
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            QueuePosition = i,
                            Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
                            ResultStatus = TaskResultStatus.Unknown,
                            ActionAfterFinish = TaskActionAfterFinish.Renew,
                            ExecutorId = freeBots[i].ID!
                        };

                        tasks.Add(task);
                    }

                    // create tasks
                    await earnSiteTasksService.CreateAsync(tasks);

                    List<WithdrawMoneyGroupSelectiveTaskWithAuth> tasksForMainAccs = new List<WithdrawMoneyGroupSelectiveTaskWithAuth>(tasks);

                    // выдача заданий мейнов ботам 
                    //for (int i = 0; i < socpublicMainAccounts.Count; i++)
                    //{
                    //    // send Task to bot
                    //    await ClientHubContext.Clients.User(freeBots[i].ID!).SendAsync("do_new_platform_socpubliccom_withdrawal_task", tasks[i]);

                    //    // update task status
                    //    await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "Status", (int)TaskStatus.Started);

                    //    // update Bot status
                    //    await clientsBotService.UpdateAsync(freeBots[i].ID!, "Status", (int)ClientStatus.AtWork);

                    //    // update Task executorId
                    //    //await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "ExecutorId", freeBots[i].ID!);
                    //}

                    tasks.Clear();

                    #endregion

                    #region Создание заданий слейвов и выдача свободным ботам их

                    string idOfTheBotAttachedToThisMainAccount = "";

                    WithdrawMoneyGroupSelectiveTaskWithAuth WithdrawMoneyTask;

                    foreach (var t in tasks_dynamic_collection)
                    {
                        if(t as WithdrawMoneyGroupSelectiveTaskWithAuth != null)
                        {
                            WithdrawMoneyTask = t;

                            if(WithdrawMoneyTask.Account!.IsMain.HasValue && WithdrawMoneyTask.Account.IsMain.Value)
                            {
                                continue;
                            }
                            else if(WithdrawMoneyTask.IsTaskCreatedOnPlatformBySlave.HasValue && WithdrawMoneyTask.IsTaskCreatedOnPlatformBySlave.Value
                                && WithdrawMoneyTask.IsTaskCompletedOnPlatformByMain.HasValue && WithdrawMoneyTask.IsTaskCompletedOnPlatformByMain.Value)
                            {
                                continue;
                            }

                            // WARNING 

                            string main_login_attached_to_this_slave = internalTasks.Where(task_ => task_.SlaveAccountWhoCreateTaskLogin == t.DatabaseMainAccountLogin).First().MainAccountWhoExecuteTaskLogin!;

                            idOfTheBotAttachedToThisMainAccount = mainAccountsAndTheirAttachedBots.Where(pair => pair.Key == main_login_attached_to_this_slave).First().Value;

                            //if (WithdrawMoneyTask.Account.Refer!.Login == "xpomocoma777")
                            //{
                            //    idOfTheBotAttachedToThisMainAccount = mainAccountsAndTheirAttachedBots.Where(pair => pair.Key == "dashshendel").First().Value;
                            //}
                            //else if (WithdrawMoneyTask.Account.Refer!.Login == "dashshendel")
                            //{
                            //    idOfTheBotAttachedToThisMainAccount = mainAccountsAndTheirAttachedBots.Where(pair => pair.Key == "ikuc365").First().Value;
                            //}
                            //else if (WithdrawMoneyTask.Account.Refer!.Login == "ikuc365")
                            //{
                            //    idOfTheBotAttachedToThisMainAccount = mainAccountsAndTheirAttachedBots.Where(pair => pair.Key == "xpomocoma777").First().Value;
                            //}

                            var task_ = new WithdrawMoneyGroupSelectiveTaskWithAuth("1", TaskFrom.Management, SocpublicComUrl.http, WithdrawMoneyTask.Account.Id!, WithdrawMoneyTask.Account, socpublicComTaskSubtypesForWithdrawal)
                            {
                                Id = ObjectId.GenerateNewId().ToString(),
                                QueuePosition = 0,
                                Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
                                ResultStatus = TaskResultStatus.Unknown,
                                ActionAfterFinish = TaskActionAfterFinish.Renew,
                                IdOfTheBotAttachedToThisMainAccount = idOfTheBotAttachedToThisMainAccount,
                                DatabaseSlaveAccountId = WithdrawMoneyTask.Account.Id,
                                DatabaseMainAccountId = WithdrawMoneyTask.DatabaseMainAccountId,
                                DatabaseSlaveAccountLogin = WithdrawMoneyTask.DatabaseSlaveAccountLogin,
                                DatabaseMainAccountLogin = WithdrawMoneyTask.DatabaseMainAccountLogin,
                                IsTaskCreatedOnPlatformBySlave = WithdrawMoneyTask.IsTaskCreatedOnPlatformBySlave is null ? false : WithdrawMoneyTask.IsTaskCreatedOnPlatformBySlave,
                                IsTaskCompletedOnPlatformByMain = WithdrawMoneyTask.IsTaskCompletedOnPlatformByMain is null ? false : WithdrawMoneyTask.IsTaskCompletedOnPlatformByMain,
                                InternalStatus = WithdrawMoneyTask.InternalStatus is WithdrawMoneyGroupSelectiveTaskWithAuth.WithdrawalInternalStatus.CompletedWithErrorByMain ? WithdrawMoneyGroupSelectiveTaskWithAuth.WithdrawalInternalStatus.CompletedSuccessfullBySlave : WithdrawMoneyTask.InternalStatus,
                                WithdrawalOfMoneyPlatformTask = WithdrawMoneyTask.WithdrawalOfMoneyPlatformTask,
                                NeedToEditPlatformTaskForSlave = WithdrawMoneyTask.IsTaskCreatedOnPlatformBySlave is true ? false : true,
                                NeedToRemovePlatformTaskForSlave = false,
                            };

                            tasks.Add(task_);
                        }
                    }

                    // create tasks
                    await earnSiteTasksService.CreateAsync(tasks);



                    ////// TEST GIVE TASKS TO MAINACCS
                    ///// выдача заданий мейнов ботам 
                    for (int i = 0; i < socpublicMainAccounts.Count; i++)
                    {
                        // send Task to bot
                        await ClientHubContext.Clients.User(freeBots[i].ID!).SendAsync("do_new_platform_socpubliccom_withdrawal_task", tasksForMainAccs[i]);

                        // update task status
                        await earnSiteTasksService.UpdateAsync(tasksForMainAccs[i].Id!, "Status", (int)TaskStatus.Started);

                        // update Bot status
                        await clientsBotService.UpdateAsync(freeBots[i].ID!, "Status", (int)ClientStatus.AtWork);

                        // update Task executorId
                        //await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "ExecutorId", freeBots[i].ID!);
                    }



                    // удаляем задания которые уже прошли первый этап вывода(созданы на платформе соцпаблик) чтобы они не передались ботам первого этапа, т.к. им уже нужен второй этап.
                    tasks.RemoveAll(t => (t.IsTaskCreatedOnPlatformBySlave.HasValue && t.IsTaskCreatedOnPlatformBySlave!.Value) || (t.InternalStatus.HasValue && t.InternalStatus == WithdrawMoneyGroupSelectiveTaskWithAuth.WithdrawalInternalStatus.CompletedSuccessfullBySlave));

                    // give tasks to bots
                    List<ModelClient> bots_for_slaves = await clientsBotService.GetAsync();

                    if (bots_for_slaves.Count > 0)
                    {
                        List<ModelClient> freeBots_for_slaves = bots_for_slaves.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.EarningSiteBot).ToList();

                        if (freeBots_for_slaves.Count > 0)
                        {
                            for (int i = 0; i < freeBots_for_slaves.Count && i < tasks.Count; i++)
                            {
                                // send Task to bot
                                await ClientHubContext.Clients.User(freeBots_for_slaves[i].ID!).SendAsync("do_new_platform_socpubliccom_withdrawal_task", tasks[i]);

                                // update task status
                                await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "Status", (int)TaskStatus.Started);

                                // update Bot status
                                await clientsBotService.UpdateAsync(freeBots_for_slaves[i].ID!, "Status", (int)ClientStatus.AtWork);

                                // update Task executorId
                                await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "ExecutorId", freeBots[i].ID!);
                            }  
                        }
                    }

                    #endregion

                    if (tasks.Count > 0)
                    {
                        // set run tasks settings
                        runTasksSettings.TasksIsRunning = true;
                        runTasksSettings.CurrentLaunchedTaskType = CommonModels.ProjectTask.LaunchTaskType.WithdrawingMoney;
                        await runTasksSettingsService.ReplaceAsync(runTasksSettings.Id!, runTasksSettings);
                    }

                    result = true;
                }
                else
                {
                    #region создание заданий мейнов и резервирование ботов под мейнов и выдача им этих заданий
                    // создание заданий мейнов и резервирование ботов под мейнов и выдача им этих заданий

                    List<Account> socpublicMainAccounts = socpublicAccounts.Where(acc => acc.IsMain.HasValue && acc.IsMain.Value).ToList();
                    if (socpublicMainAccounts.Count == 0)
                        return false;

                    socpublicAccounts.RemoveAll(acc => acc.IsMain.HasValue && acc.IsMain.Value);

                    //remove after !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!-------------------------
                    socpublicMainAccounts.RemoveAll(acc => acc.Login == "xpomocoma777");

                    List<KeyValuePair<string, string>> mainAccountsAndTheirAttachedBots = new List<KeyValuePair<string, string>>();

                    // get free bots for reserve
                    List<ModelClient> bots = await clientsBotService.GetAsync();

                    if (bots.Count == 0 || bots.Count < socpublicMainAccounts.Count)
                        return false;

                    List<ModelClient> freeBots = bots.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.EarningSiteBot).ToList();

                    if (freeBots.Count == 0 || freeBots.Count < socpublicMainAccounts.Count)
                        return false;

                    for (int i = 0; i < socpublicMainAccounts.Count; i++)
                    {
                        // attach main_acc with free bot
                        mainAccountsAndTheirAttachedBots.Add(new KeyValuePair<string, string>(socpublicMainAccounts[i].Login!, freeBots[i].ID!));

                        WithdrawMoneyGroupSelectiveTaskWithAuth task = new WithdrawMoneyGroupSelectiveTaskWithAuth("1", TaskFrom.Management, SocpublicComUrl.https, socpublicMainAccounts[i].Id!, socpublicMainAccounts[i], socpublicComTaskSubtypesForWithdrawal)
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            QueuePosition = i,
                            Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
                            ResultStatus = TaskResultStatus.Unknown,
                            ActionAfterFinish = TaskActionAfterFinish.Renew,
                            ExecutorId = freeBots[i].ID!
                        };

                        tasks.Add(task);
                    }

                    // create tasks
                    await earnSiteTasksService.CreateAsync(tasks);

                    List<WithdrawMoneyGroupSelectiveTaskWithAuth> tasksForMainAccs = new List<WithdrawMoneyGroupSelectiveTaskWithAuth>(tasks);

                    //// выдача заданий мейнов ботам 
                    //for (int i = 0; i < socpublicMainAccounts.Count; i++)
                    //{
                    //    // send Task to bot
                    //    await ClientHubContext.Clients.User(freeBots[i].ID!).SendAsync("do_new_platform_socpubliccom_withdrawal_task", tasks[i]);

                    //    // update task status
                    //    await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "Status", (int)TaskStatus.Started);

                    //    // update Bot status
                    //    await clientsBotService.UpdateAsync(freeBots[i].ID!, "Status", (int)ClientStatus.AtWork);

                    //    // update Task executorId
                    //    //await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "ExecutorId", freeBots[i].ID!);
                    //}

                    tasks.Clear();

                    #endregion

                    #region Создание заданий слейвов и выдача свободным ботам их

                    string idOfTheBotAttachedToThisMainAccount = "";
                    string databaseMainAccountId = "";
                    string databaseMainAccountLogin = "";

                    for (int i = 0; i < socpublicAccounts.Count; i++)
                    {
                        if (socpublicAccounts[i].Login == "xpomocoma777" || socpublicAccounts[i].Login == "ikuc365" || socpublicAccounts[i].Login == "dashshendel")
                        {
                            continue;
                        }

                        // WARNING

                        string main_login_attached_to_this_slave = internalTasks.Where(t => t.SlaveAccountWhoCreateTaskLogin == socpublicAccounts[i].Login).First().MainAccountWhoExecuteTaskLogin!;

                        if(main_login_attached_to_this_slave == "dashshendel")
                        {
                            continue;
                        }

                        idOfTheBotAttachedToThisMainAccount = mainAccountsAndTheirAttachedBots.Where(pair => pair.Key == main_login_attached_to_this_slave).First().Value;
                        databaseMainAccountId = socpublicMainAccounts.Where(acc => acc.Login == main_login_attached_to_this_slave).First().Id!;
                        databaseMainAccountLogin = socpublicMainAccounts.Where(acc => acc.Login == main_login_attached_to_this_slave).First().Login!;

                        //if (socpublicAccounts[i].Refer!.Login == "xpomocoma777")
                        //{
                        //    idOfTheBotAttachedToThisMainAccount = mainAccountsAndTheirAttachedBots.Where(pair => pair.Key == "dashshendel").First().Value;
                        //    databaseMainAccountId = socpublicMainAccounts.Where(acc => acc.Login == "dashshendel").First().Id!;
                        //    databaseMainAccountLogin = socpublicMainAccounts.Where(acc => acc.Login == "dashshendel").First().Login!;
                        //}
                        //else if (socpublicAccounts[i].Refer!.Login == "dashshendel")
                        //{
                        //    idOfTheBotAttachedToThisMainAccount = mainAccountsAndTheirAttachedBots.Where(pair => pair.Key == "ikuc365").First().Value;
                        //    databaseMainAccountId = socpublicMainAccounts.Where(acc => acc.Login == "ikuc365").First().Id!;
                        //    databaseMainAccountLogin = socpublicMainAccounts.Where(acc => acc.Login == "ikuc365").First().Login!;
                        //}
                        //else if (socpublicAccounts[i].Refer!.Login == "ikuc365")
                        //{
                        //    idOfTheBotAttachedToThisMainAccount = idOfTheBotAttachedToThisMainAccount = mainAccountsAndTheirAttachedBots.Where(pair => pair.Key == "xpomocoma777").First().Value;
                        //    databaseMainAccountId = socpublicMainAccounts.Where(acc => acc.Login == "xpomocoma777").First().Id!;
                        //    databaseMainAccountLogin = socpublicMainAccounts.Where(acc => acc.Login == "xpomocoma777").First().Login!;
                        //}

                        WithdrawMoneyGroupSelectiveTaskWithAuth task = new WithdrawMoneyGroupSelectiveTaskWithAuth("1", TaskFrom.Management, SocpublicComUrl.http, socpublicAccounts[i].Id!, socpublicAccounts[i], socpublicComTaskSubtypesForWithdrawal)
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            QueuePosition = i,
                            Status = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus.Created,
                            ResultStatus = TaskResultStatus.Unknown,
                            ActionAfterFinish = TaskActionAfterFinish.Renew,
                            IdOfTheBotAttachedToThisMainAccount = idOfTheBotAttachedToThisMainAccount,
                            DatabaseSlaveAccountId = socpublicAccounts[i].Id!,
                            DatabaseMainAccountId = databaseMainAccountId,
                            DatabaseSlaveAccountLogin = socpublicAccounts[i].Login,
                            DatabaseMainAccountLogin = databaseMainAccountLogin,
                            IsTaskCreatedOnPlatformBySlave = false,
                            IsTaskCompletedOnPlatformByMain = false,
                            NeedToEditPlatformTaskForSlave = false,
                            NeedToRemovePlatformTaskForSlave = false,
                        };

                        tasks.Add(task);
                    }

                    // create tasks
                    await earnSiteTasksService.CreateAsync(tasks);





                    ////// TEST GIVE TASKS TO MAINACCS
                    ///// выдача заданий мейнов ботам 
                    for (int i = 0; i < socpublicMainAccounts.Count; i++)
                    {
                        // send Task to bot
                        await ClientHubContext.Clients.User(freeBots[i].ID!).SendAsync("do_new_platform_socpubliccom_withdrawal_task", tasksForMainAccs[i]);

                        // update task status
                        await earnSiteTasksService.UpdateAsync(tasksForMainAccs[i].Id!, "Status", (int)TaskStatus.Started);

                        // update Bot status
                        await clientsBotService.UpdateAsync(freeBots[i].ID!, "Status", (int)ClientStatus.AtWork);

                        // update Task executorId
                        //await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "ExecutorId", freeBots[i].ID!);
                    }





                    // give tasks to bots
                    List<ModelClient> bots_for_slaves = await clientsBotService.GetAsync();

                    if (bots_for_slaves.Count == 0)
                        return false;

                    List<ModelClient> freeBots_for_slaves = bots_for_slaves.Where(bot => bot.Status == CommonModels.Client.Client.ClientStatus.Free && bot.Role == BotRole.EarningSiteBot).ToList();

                    for (int i = 0; i < freeBots_for_slaves.Count && i < tasks.Count; i++)
                    {
                        // send Task to bot
                        await ClientHubContext.Clients.User(freeBots_for_slaves[i].ID!).SendAsync("do_new_platform_socpubliccom_withdrawal_task", tasks[i]);

                        // update task status
                        await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "Status", (int)TaskStatus.Started);

                        // update Bot status
                        await clientsBotService.UpdateAsync(freeBots_for_slaves[i].ID!, "Status", (int)ClientStatus.AtWork);

                        // update Task executorId
                        await earnSiteTasksService.UpdateAsync(tasks[i].Id!, "ExecutorId", freeBots[i].ID!);
                    }

                    #endregion

                    // set run tasks settings
                    runTasksSettings.TasksIsRunning = true;
                    runTasksSettings.CurrentLaunchedTaskType = CommonModels.ProjectTask.LaunchTaskType.WithdrawingMoney;
                    await runTasksSettingsService.ReplaceAsync(runTasksSettings.Id!, runTasksSettings);

                    result = true;
                }
            }
            catch (Exception e) { }


            return result;
        }



        public async Task<DeletedTasks?> DeleteTasks(DeleteTasks deleteTasks)
        {
            DeletedTasks? deletedTasks = null;

            try
            {
                if (deleteTasks.Tasks.Count > 0)
                {
                    // delete tasks
                    List<string> tasksIdList = new List<string>();
                    foreach (var task_ in deleteTasks.Tasks)
                    {
                        var task = JsonConvert.DeserializeObject<dynamic>(task_.ToString()!);
                        string task_id = task.id!;

                        tasksIdList.Add(task_id);
                    }

                    await earnSiteTasksService.DeleteAsync(tasksIdList);
                }
            }
            catch
            {
                deletedTasks = new DeletedTasks("Failed delete tasks", false);
            }

            return deletedTasks = new DeletedTasks("Tasks deleted successfully", true);
        }

    }
}
