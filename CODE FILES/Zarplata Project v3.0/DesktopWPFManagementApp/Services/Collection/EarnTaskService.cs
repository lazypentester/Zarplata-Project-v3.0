using CommonModels.Client.Models.DeleteBotsModels;
using CommonModels.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.Client.Client;
using CommonModels.Client.Models.SearchBotsModels;
using System.Threading;
using CommonModels.ProjectTask.EarningSite.SearchTasksModels;
using CommonModels.ProjectTask.EarningSite.DeleteTasksModels;

namespace DesktopWPFManagementApp.Services.Collection
{
    public class EarnTaskService
    {
        private HubConnection HUB_CONNECTION { get; set; }

        // events
        public event Action<dynamic> NewTaskCreated = (task) => { };
        public event Action<dynamic> TasksDeleted = (deletedTasks) => { };
        //public event Action<List<string>, ClientStatus> BotsStatusChanged = (botsIdList, changedToStatus) => { };

        public EarnTaskService(HubConnection hUB_CONNECTION)
        {
            HUB_CONNECTION = hUB_CONNECTION;

            RegisterHubEventHandlers();
        }

        public void RegisterHubEventHandlers()
        {
            //HUB_CONNECTION.On<ModelClient>("new_client_bot_connected", (client_bot) =>
            //{
            //    NewBotConnected.Invoke(client_bot);
            //});

            //HUB_CONNECTION.On<string>("bot_closed", (client_bot_id) =>
            //{
            //    BotClosed.Invoke(client_bot_id);
            //});

            //HUB_CONNECTION.On<DeleteBots>("bots_deleted", (deletedBots) =>
            //{
            //    BotsDeleted.Invoke(deletedBots);
            //});

            //HUB_CONNECTION.On<List<string>>("bots_changed_status_to_free", (botsIdList) =>
            //{
            //    BotsStatusChanged.Invoke(botsIdList, ClientStatus.Free);
            //});
            //HUB_CONNECTION.On<List<string>>("bots_changed_status_to_atwork", (botsIdList) =>
            //{
            //    BotsStatusChanged.Invoke(botsIdList, ClientStatus.AtWork);
            //});
            //HUB_CONNECTION.On<List<string>>("bots_changed_status_to_stopped", (botsIdList) =>
            //{
            //    BotsStatusChanged.Invoke(botsIdList, ClientStatus.Stopped);
            //});
        }

        public async Task CreateAndStartEarnTasks(CancellationToken cancellationToken)
        {
            try
            {
                await HUB_CONNECTION.SendAsync("create_and_start_earn_tasks", cancellationToken);
            }
            catch { }
        }

        public async Task CreateAndStartAutoregTasks(CancellationToken cancellationToken)
        {
            try
            {
                await HUB_CONNECTION.SendAsync("create_and_start_autoreg_tasks", cancellationToken);
            }
            catch { }
        }

        public async Task CreateAndStartWithdrawalOfMoneyTasks(CancellationToken cancellationToken)
        {
            try
            {
                await HUB_CONNECTION.SendAsync("create_and_start_withdrawal_of_money_tasks", cancellationToken);
            }
            catch { }
        }

        public async Task<FoundedTasks?> GetTasksCollection(FindTasks findTasks, CancellationToken cancellationToken)
        {
            FoundedTasks? foundedTasks = null;

            try
            {
                foundedTasks = await HUB_CONNECTION.InvokeAsync<FoundedTasks?>("get_tasks_collection", findTasks, cancellationToken);
            }
            catch(Exception e)
            {

            }

            return foundedTasks;
        }

        public async Task<DeletedTasks?> DeleteTasks(DeleteTasks deleteTasks, CancellationToken cancellationToken)
        {
            DeletedTasks? deletedTasks = null;

            try
            {
                deletedTasks = await HUB_CONNECTION.InvokeAsync<DeletedTasks?>("delete_tasks", deleteTasks, cancellationToken);
            }
            catch { }

            return deletedTasks;
        }
    }
}
