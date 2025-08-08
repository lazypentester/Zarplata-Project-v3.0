using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.Client.Models.SearchBotsModels.SearchBotEnums;
using static CommonModels.ProjectTask.EarningSite.SearchTasksModels.SearchTaskEnums;

namespace CommonModels.ProjectTask.EarningSite.SearchTasksModels.FilterModels
{
    public class FindFilterTasks
    {
        public KeyValuePair<string, string> FindSearchKeyword { get; set; }
        public DateTime? FindSearchKeywordDateTime { get; set; } = null;
        public Dictionary<string, List<int>> FindFiltersInTypeInt { get; set; }
        public Dictionary<string, List<string>> FindFiltersInTypeString { get; set; }

        public FindFilterTasks()
        {
            FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersEarnSiteTasks.Id.ToString(), "");
            FindFiltersInTypeInt = new Dictionary<string, List<int>>();
            FindFiltersInTypeString = new Dictionary<string, List<string>>();
        }
    }
}
