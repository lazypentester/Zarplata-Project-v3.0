using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.EarningSite.DeleteTasksModels
{
    public class DeletedTasks
    {
        public string DeletedTasksStatusText { get; set; }
        public bool DeletedTasksStatus { get; set; }

        public DeletedTasks(string deletedTasksStatusText, bool deletedTasksStatus)
        {
            DeletedTasksStatusText = deletedTasksStatusText;
            DeletedTasksStatus = deletedTasksStatus;
        }
    }
}
