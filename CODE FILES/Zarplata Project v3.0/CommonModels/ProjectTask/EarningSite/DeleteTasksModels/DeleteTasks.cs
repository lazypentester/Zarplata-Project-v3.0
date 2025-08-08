using CommonModels.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.EarningSite.DeleteTasksModels
{
    public class DeleteTasks
    {
        public List<dynamic> Tasks { get; set; }

        public DeleteTasks(List<dynamic> tasks)
        {
            Tasks = tasks;
        }
    }
}
