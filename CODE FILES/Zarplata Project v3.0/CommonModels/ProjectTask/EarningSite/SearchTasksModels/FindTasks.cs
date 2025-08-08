using CommonModels.Client.Models.SearchBotsModels.FilterModels;
using CommonModels.ProjectTask.EarningSite.SearchTasksModels.FilterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.Client.Models.SearchBotsModels.SearchBotEnums;
using static CommonModels.ProjectTask.EarningSite.SearchTasksModels.SearchTaskEnums;

namespace CommonModels.ProjectTask.EarningSite.SearchTasksModels
{
    public class FindTasks
    {
        public int SelectedPageNumber { get; set; }
        public int ResultsPerPage { get; set; }
        public FindSortEarnSiteTasks FindSortTasks { get; set; }
        public FindFilterTasks FindFilterTasks { get; set; }

        public FindTasks(int selectedPageNumber, int resultsPerPage, FindSortEarnSiteTasks findSortTasks, FindFilterTasks findFilterTasks)
        {
            SelectedPageNumber = selectedPageNumber;
            ResultsPerPage = resultsPerPage;
            FindSortTasks = findSortTasks;
            FindFilterTasks = findFilterTasks;
        }
    }
}
