using CommonModels.Client.Models;
using CommonModels.Client.Models.DeleteBotsModels;
using CommonModels.Client.Models.SearchBotsModels;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CommonModels.Client.Client;

namespace DesktopWPFManagementApp.Services.Collection
{
    public class BotService
    {
        private HubConnection HUB_CONNECTION { get; set; }

        // events
        public event Action<ModelClient> NewBotConnected = (client_bot) => { };
        public event Action<string> BotClosed = (client_bot_id) => { };
        public event Action<DeleteBots> BotsDeleted = (deletedBots) => { };
        public event Action<List<string>, ClientStatus> BotsStatusChanged = (botsIdList, changedToStatus) => { };

        public BotService(HubConnection hUB_CONNECTION)
        {
            HUB_CONNECTION = hUB_CONNECTION;

            RegisterHubEventHandlers();
        }

        public void RegisterHubEventHandlers()
        {
            HUB_CONNECTION.On<ModelClient>("new_client_bot_connected", (client_bot) =>
            {
                NewBotConnected.Invoke(client_bot);
            });

            HUB_CONNECTION.On<string>("bot_closed", (client_bot_id) =>
            {
                BotClosed.Invoke(client_bot_id);
            });

            HUB_CONNECTION.On<DeleteBots>("bots_deleted", (deletedBots) =>
            {
                BotsDeleted.Invoke(deletedBots);
            });

            HUB_CONNECTION.On<List<string>>("bots_changed_status_to_free", (botsIdList) =>
            {
                BotsStatusChanged.Invoke(botsIdList, ClientStatus.Free);
            });
            HUB_CONNECTION.On<List<string>>("bots_changed_status_to_atwork", (botsIdList) =>
            {
                BotsStatusChanged.Invoke(botsIdList, ClientStatus.AtWork);
            });
            HUB_CONNECTION.On<List<string>>("bots_changed_status_to_stopped", (botsIdList) =>
            {
                BotsStatusChanged.Invoke(botsIdList, ClientStatus.Stopped);
            });
        }

        public async Task<FoundedBots?> GetBotsCollection(FindBots findBots, CancellationToken cancellationToken)
        {
            FoundedBots? foundedBots = null;

            try
            {
                foundedBots = await HUB_CONNECTION.InvokeAsync<FoundedBots?>("get_bots_collection", findBots, cancellationToken);
            }
            catch { }

            return foundedBots;
        }

        public async Task<bool> StartBots(List<string> botsIdList, CancellationToken cancellationToken)
        {
            bool successfull = false;

            try
            {
                successfull = await HUB_CONNECTION.InvokeAsync<bool>("start_bots", botsIdList, cancellationToken);
            }
            catch { }

            return successfull;
        }

        public async Task<bool> StopBots(List<string> botsIdList, CancellationToken cancellationToken)
        {
            bool successfull = false;

            try
            {
                successfull = await HUB_CONNECTION.InvokeAsync<bool>("stop_bots", botsIdList, cancellationToken);
            }
            catch { }

            return successfull;
        }

        public async Task<DeletedBots?> DeleteBots(DeleteBots deleteBots, CancellationToken cancellationToken)
        {
            DeletedBots? deletedBots = null;

            try
            {
                deletedBots = await HUB_CONNECTION.InvokeAsync<DeletedBots?>("delete_bots", deleteBots, cancellationToken);
            }
            catch { }

            return deletedBots;
        }
    }
}
