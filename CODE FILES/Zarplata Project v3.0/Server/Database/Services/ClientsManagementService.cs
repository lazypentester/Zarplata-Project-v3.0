using CommonModels.Client.Models;
using CommonModels.Client.Models.DeleteBotsModels;
using CommonModels.Client.Models.SearchBotsModels;
using Microsoft.AspNetCore.SignalR;
using Server.Hubs;

namespace Server.Database.Services
{
    public class ClientsManagementService
    {
        private readonly ClientsService clientsService;
        private readonly BlockedMachinesService blockedMachinesService;
        private readonly IHubContext<ManagementHub> ManagementHubContext;

        public ClientsManagementService(ClientsService clientsService, BlockedMachinesService blockedMachinesService, IHubContext<ManagementHub> managementHubContext)
        {
            this.clientsService = clientsService;
            this.blockedMachinesService = blockedMachinesService;
            ManagementHubContext = managementHubContext;
        }

        public async Task<FoundedBots?> GetBotsCollection(FindBots findBots)
        {
            FoundedBots? botsFinalCollection = null;
            List<ModelClient>? bots = new List<ModelClient>();

            // find collection by filters
            try
            {
                bots.AddRange(await clientsService.GetWithFiltersAndSortByRegistrationDateAsync(
                        findBots.FindFilterBots.FindFiltersInTypeInt,
                        findBots.FindFilterBots.FindFiltersInTypeString,
                        findBots.FindFilterBots.FindSearchKeyword,
                        findBots.FindFilterBots.FindSearchKeywordDateTime,
                        findBots.FindSortBots
                        ));
            }
            catch { }

            // разбиваем коллекцию на страницы и берем текущую выбраную страницу
            if (bots != null)
            {
                try
                {
                    double pageCount = (double)bots.Count / (double)findBots.ResultsPerPage;
                    double pageCountRounded = Math.Round(pageCount, 0, MidpointRounding.ToPositiveInfinity);
                    int firstElementNumOnSelectedPage = 0;
                    int lastElementNumOnSelectedPage = 0;
                    int allBotsCount = bots.Count;

                    if (findBots.SelectedPageNumber > (int)pageCountRounded)
                    {
                        findBots.SelectedPageNumber = (int)pageCountRounded;
                    }

                    // отсеиваем лишних ботов, оставляем только одну страницу
                    List<ModelClient> onePageBots = bots;
                    if ((int)pageCountRounded > 1)
                    {
                        try
                        {
                            int countElementsOnLastPageOfAllPages = bots.Count - (((int)pageCountRounded - 1) * findBots.ResultsPerPage);

                            if (findBots.SelectedPageNumber == 1)
                            {
                                firstElementNumOnSelectedPage = 1;
                                lastElementNumOnSelectedPage = findBots.ResultsPerPage;

                                onePageBots.RemoveRange(findBots.ResultsPerPage, (bots.Count - findBots.ResultsPerPage));
                            }
                            else if (findBots.SelectedPageNumber == (pageCountRounded))
                            {
                                firstElementNumOnSelectedPage = bots.Count - countElementsOnLastPageOfAllPages + 1;
                                lastElementNumOnSelectedPage = bots.Count;

                                onePageBots.RemoveRange(0, ((findBots.SelectedPageNumber - 1) * findBots.ResultsPerPage));
                            }
                            else
                            {
                                firstElementNumOnSelectedPage = ((findBots.SelectedPageNumber - 1) * findBots.ResultsPerPage) + 1;
                                lastElementNumOnSelectedPage = findBots.SelectedPageNumber * findBots.ResultsPerPage;

                                onePageBots.RemoveRange((findBots.SelectedPageNumber * findBots.ResultsPerPage), (bots.Count - (findBots.SelectedPageNumber * findBots.ResultsPerPage)));
                                onePageBots.RemoveRange(0, ((findBots.SelectedPageNumber * findBots.ResultsPerPage) - findBots.ResultsPerPage));
                            }
                        }
                        catch
                        {
                            onePageBots = new List<ModelClient>();
                        }
                    }
                    else
                    {
                        firstElementNumOnSelectedPage = 1;
                        lastElementNumOnSelectedPage = bots.Count;
                    }

                    botsFinalCollection = new FoundedBots(
                        onePageBots,
                        findBots.SelectedPageNumber,
                        allBotsCount,
                        (int)pageCountRounded,
                        firstElementNumOnSelectedPage,
                        lastElementNumOnSelectedPage,
                        "Боты успешно найдены",
                        true);
                }
                catch { }
            }

            return botsFinalCollection;
        }

        public async Task<bool> StartBots(List<string> botsIdList)
        {
            bool successfull = false;

            try
            {
                if(botsIdList.Count > 0)
                {
                    successfull = await clientsService.ChangeBotsStatusToFree(botsIdList);
                }
            }
            catch { }

            return successfull;
        }

        public async Task<bool> StopBots(List<string> botsIdList)
        {
            bool successfull = false;

            try
            {
                if (botsIdList.Count > 0)
                {
                    successfull = await clientsService.ChangeBotsStatusToStopped(botsIdList);
                }
            }
            catch { }

            return successfull;
        }

        public async Task<DeletedBots?> DeleteBots(DeleteBots deleteBots)
        {
            DeletedBots? deletedBots = null;

            try
            {
                if (deleteBots.Bots.Count > 0)
                {
                    // block machines if requires
                    if (deleteBots.BlockBotsMachines)
                    {
                        List<ModelBlockedMachine> machinesToBlock = new List<ModelBlockedMachine>();

                        foreach(var bot in deleteBots.Bots)
                        {
                            if (machinesToBlock.Where(machineTB => machineTB.IP == bot.IP && machineTB.MACHINE.IDENTITY_KEY == bot.MACHINE.IDENTITY_KEY).Count() == 0)
                            {
                                machinesToBlock.Add(new ModelBlockedMachine()
                                {
                                    IP = bot.IP,
                                    MACHINE = bot.MACHINE
                                }); 
                            }
                        }

                        // remove already existing machines in block list
                        foreach (var alreadyBlockedBot in await blockedMachinesService.GetAsync())
                        {
                            ModelBlockedMachine? currentMachineToBlockRepeat = machinesToBlock.Where(item => item.IP == alreadyBlockedBot.IP && item.MACHINE.IDENTITY_KEY == alreadyBlockedBot.MACHINE.IDENTITY_KEY).FirstOrDefault();

                            if (currentMachineToBlockRepeat != null)
                            {
                                machinesToBlock.Remove(currentMachineToBlockRepeat);
                            }
                        }

                        // block bots
                        await blockedMachinesService.CreateAsync(machinesToBlock);
                    }

                    // delete bots
                    List<string> botsIdList = new List<string>();
                    foreach (var bot in deleteBots.Bots)
                    {
                        botsIdList.Add(bot.ID!);
                    }

                    await clientsService.DeleteAsync(botsIdList);
                }
            }
            catch
            {
                deletedBots = new DeletedBots("Failed delete bots", false);
            }

            return deletedBots = new DeletedBots("Bots deleted successfully", true); ;
        }
        public async Task DeleteBotsWithoutCheckBlock(List<ModelClient> botsToDelete)
        {
            // delete bots
            try
            {
                List<string> botsIdList = new List<string>();
                foreach (var bot in botsToDelete)
                {
                    botsIdList.Add(bot.ID!);
                }

                await clientsService.DeleteAsync(botsIdList);
            }
            catch { }
        }
    }
}
