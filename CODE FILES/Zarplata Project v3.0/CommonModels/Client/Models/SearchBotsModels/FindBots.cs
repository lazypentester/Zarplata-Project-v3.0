using CommonModels.Client.Models.SearchBotsModels.FilterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.Client.Models.SearchBotsModels.SearchBotEnums;

namespace CommonModels.Client.Models.SearchBotsModels
{
    public class FindBots
    {
        public int SelectedPageNumber { get; set; }
        public int ResultsPerPage { get; set; }
        public FindSortBots FindSortBots { get; set; }
        public FindFilterBots FindFilterBots { get; set; }

        public FindBots(int selectedPageNumber, int resultsPerPage, FindSortBots findSortBots, FindFilterBots findFilterBots)
        {
            SelectedPageNumber = selectedPageNumber;
            ResultsPerPage = resultsPerPage;
            FindSortBots = findSortBots;
            FindFilterBots = findFilterBots;
        }
    }
}
