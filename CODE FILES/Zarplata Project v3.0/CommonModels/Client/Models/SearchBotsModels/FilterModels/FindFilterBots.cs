using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.Client.Models.SearchBotsModels.SearchBotEnums;

namespace CommonModels.Client.Models.SearchBotsModels.FilterModels
{
    public class FindFilterBots
    {
        public KeyValuePair<string, string> FindSearchKeyword { get; set; }
        public DateTime? FindSearchKeywordDateTime { get; set; } = null;
        public Dictionary<string, List<int>> FindFiltersInTypeInt { get; set; }
        public Dictionary<string, List<string>> FindFiltersInTypeString { get; set; }

        public FindFilterBots()
        {
            FindSearchKeyword = new KeyValuePair<string, string>(FindSearchKeywordParametersBots.Id.ToString(), "");
            FindFiltersInTypeInt = new Dictionary<string, List<int>>();
            FindFiltersInTypeString = new Dictionary<string, List<string>>();
        }
    }
}
