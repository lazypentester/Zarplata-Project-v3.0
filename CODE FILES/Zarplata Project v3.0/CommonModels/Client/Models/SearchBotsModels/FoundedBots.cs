using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Client.Models.SearchBotsModels
{
    public class FoundedBots
    {
        public int SelectedPageNumber { get; set; }
        public List<ModelClient> BotClients { get; set; }
        public int AllPagesCount { get; set; }
        public int AllBotClientsCount { get; set; }
        public int FirstElementNumberOfCurrentPage { get; set; }
        public int LastElementNumberOfCurrentPage { get; set; }
        public string FoundedBotsStatusText { get; set; }
        public bool FoundedBotsStatus { get; set; }

        public FoundedBots(List<ModelClient> botClients, int selectedPageNumber, int allBotClientsCount, int allPagesCount, int firstElementNumberOfCurrentPage, int lastElementNumberOfCurrentPage, string foundedBotsStatusText, bool foundedBotsStatus)
        {
            BotClients = botClients;
            SelectedPageNumber = selectedPageNumber;
            AllBotClientsCount = allBotClientsCount;
            AllPagesCount = allPagesCount;
            FirstElementNumberOfCurrentPage = firstElementNumberOfCurrentPage;
            LastElementNumberOfCurrentPage = lastElementNumberOfCurrentPage;
            FoundedBotsStatusText = foundedBotsStatusText;
            FoundedBotsStatus = foundedBotsStatus;
        }
    }
}
