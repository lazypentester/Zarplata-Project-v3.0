using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Client.Models.DeleteBotsModels
{
    public class DeletedBots
    {
        public string DeletedBotsStatusText { get; set; }
        public bool DeletedBotsStatus { get; set; }

        public DeletedBots(string deletedBotsStatusText, bool deletedBotsStatus)
        {
            DeletedBotsStatusText = deletedBotsStatusText;
            DeletedBotsStatus = deletedBotsStatus;
        }
    }
}
