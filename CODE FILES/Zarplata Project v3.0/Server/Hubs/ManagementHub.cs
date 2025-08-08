using CommonModels;
using CommonModels.Client.Models;
using CommonModels.Client.Models.DeleteBotsModels;
using CommonModels.Client.Models.SearchBotsModels;
using CommonModels.ProjectTask.EarningSite.DeleteTasksModels;
using CommonModels.ProjectTask.EarningSite.SearchTasksModels;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth;
using CommonModels.User.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Attributes.Authorization;
using Server.Database.Services;

namespace Server.Hubs
{
    public class ManagementHub : Hub
    {
        private readonly IHubContext<ClientHub> ClientHubContext;
        private readonly ClientsManagementService clientsManagementService;
        private readonly EarnSiteTasksManagementService earnSiteTasksManagementService;
        private readonly ClientsService clientsService;

        public ManagementHub(IHubContext<ClientHub> clientHubContext, ClientsManagementService clientsManagementService, ClientsService clientsService, EarnSiteTasksManagementService earnSiteTasksManagementService)
        {
            this.ClientHubContext = clientHubContext;
            this.clientsManagementService = clientsManagementService;
            this.clientsService = clientsService;
            this.earnSiteTasksManagementService = earnSiteTasksManagementService;
        }

        #region МЕТОДЫ ВЫЗЫВАЕМЫЕ ПРИ ПОДКЛЮЧЕНИИ И ОТКЛЮЧЕНИИ КЛИЕНТОВ

        public override async Task OnConnectedAsync()
        {

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {

            await base.OnDisconnectedAsync(exception);
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)}")]
        [HubAuthorizeIP]
        public async Task addUserToAdminHubGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserRole.Admin.ToString());
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.Spectator)}")]
        [HubAuthorizeIP]
        public async Task addUserToSpectatorHubGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserRole.Spectator.ToString());
        }

        #endregion

        #region  Методы взаимодействия с заданиями типа "PlatformTask"

        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.Spectator)}")]
        [HubAuthorizeIP]
        public async Task create_and_start_autoreg_tasks()
        {
            await earnSiteTasksManagementService.CreateMassTaskSocpublicAutoreg();
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.Spectator)}")]
        [HubAuthorizeIP]
        public async Task create_and_start_earn_tasks()
        {
            await earnSiteTasksManagementService.CreateMassTaskSocpublicEarn();
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.Spectator)}")]
        [HubAuthorizeIP]
        public async Task create_and_start_withdrawal_of_money_tasks()
        {
            await earnSiteTasksManagementService.CreateMassTaskSocpublicGetMoney();
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.Spectator)}")]
        [HubAuthorizeIP]
        public async Task<FoundedTasks?> get_tasks_collection(FindTasks findTasks)
        {
            return await earnSiteTasksManagementService.GetTasksCollection(findTasks);
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.Spectator)}")]
        [HubAuthorizeIP]
        public async Task<DeletedTasks?> delete_tasks(DeleteTasks deleteTasks)
        {
            DeletedTasks? deletedTasks = null;

            deletedTasks = await earnSiteTasksManagementService.DeleteTasks(deleteTasks);

            if (deletedTasks != null && deletedTasks.DeletedTasksStatus)
            {
                // уведомление других менеджерских клиентов об удаленных заданиях
                try
                {
                    await Clients.Others.SendAsync("tasks_deleted", deletedTasks);
                }
                catch { }
            }

            return deletedTasks;
        }

        //public async Task do_new_platform_socpubliccom_task(GroupSelectiveTaskWithAuth task)
        //{
        //    await ClientHubContext.Clients.Group("Bot#Free").SendAsync("do_new_platform_socpubliccom_task", task);
        //}

        #region Методы взаимодействия с заданиями платформы типа "PlatformSocpublicComDataType"
        /*public async Task do_platform_task_socpublic_com_data_type(PlatformTask<PlatformSocpublicComDataType<object>> task)
        {
            switch (task.Data.Subtype)
            {
                case ProjectTaskEnums.PlatformSocpublicComTaskSubtype.TaskVisitWithoutTimer:
                    //(task.Data.AdditionalData as TaskVisitWithoutTimerAddDataType)!.hi = "";
                    break;
            }
        }*/
        #endregion

        #endregion

        #region Методы взаимодействия с Ботами

        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.Spectator)}")]
        [HubAuthorizeIP]
        public async Task<FoundedBots?> get_bots_collection(FindBots findBots)
        {
            return await clientsManagementService.GetBotsCollection(findBots);
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)}")]
        [HubAuthorizeIP]
        public async Task<bool> start_bots(List<string> botsIdList)
        {
            bool successfull = false;

            successfull = await clientsManagementService.StartBots(botsIdList);

            if (successfull)
            {
                try
                {
                    await Clients.Others.SendAsync("bots_changed_status_to_free", botsIdList);
                }
                catch { }
            }

            return successfull;
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)}")]
        [HubAuthorizeIP]
        public async Task<bool> stop_bots(List<string> botsIdList)
        {
            bool successfull = false;

            successfull = await clientsManagementService.StopBots(botsIdList);

            if (successfull)
            {
                try
                {
                    await Clients.Others.SendAsync("bots_changed_status_to_stopped", botsIdList);
                }
                catch { }
            }

            return successfull;
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)}")]
        [HubAuthorizeIP]
        public async Task<DeletedBots?> delete_bots(DeleteBots deleteBots)
        {
            DeletedBots? deletedBots = null;

            deletedBots = await clientsManagementService.DeleteBots(deleteBots);

            if (deletedBots != null && deletedBots.DeletedBotsStatus)
            {
                // уведомление других менеджерских клиентов об удаленных ботах
                try
                {
                    await Clients.Others.SendAsync("bots_deleted", deleteBots);
                }
                catch { }

                // уведомление самого бота (списком удаленных ботов) о его удалении из системы
                try
                {
                    List<string> deletedBotsIds = deleteBots.Bots.Select(bot => bot.ID!).ToList();

                    await ClientHubContext.Clients.Users(deletedBotsIds).SendAsync("bot_deleted");
                }
                catch { }

                // если стоит настройка "блокировка компьютера бота", проверяем на наличие других ботов на этом комп. и удаляем их,
                // и так же уведомляем об этом других менеджерских клиентов
                if (deleteBots.BlockBotsMachines)
                {
                    try
                    {
                        // удаление из бд клиентов связанных с заблокированными компами
                        List<string> machineIdentityKeys = new List<string>();
                        foreach(var bot in deleteBots.Bots)
                        {
                            machineIdentityKeys.Add(bot.MACHINE.IDENTITY_KEY!);
                        }

                        List<ModelClient>? botsByIkey = await clientsService.GetByMachineIdentityKeyAsync(machineIdentityKeys);

                        if(botsByIkey != null && botsByIkey.Any())
                        {
                            await clientsManagementService.DeleteBotsWithoutCheckBlock(botsByIkey);

                            // уведомление других менеджерских приложений об удалении доп.ботов
                            await Clients.Others.SendAsync("bots_deleted", botsByIkey);

                            // уведомление самого бота (списком удаленных ботов) о его удалении из системы
                            List<string> additionalDeletedBotsIds = botsByIkey.Select(bot => bot.ID!).ToList();

                            await ClientHubContext.Clients.Users(additionalDeletedBotsIds).SendAsync("bot_deleted");
                        }
                    }
                    catch { }
                }
            }

            return deletedBots;
        }

        #endregion
    }
}
