using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth.SelectiveTask.TaskVisitWithoutTimer.Models
{
    public class TaskVisitWithoutTimerModel
    {
        public string Number { get; set; } = "Number";
        public string Provider { get; set; } = "Provider";
        public string Name { get; set; } = "Task Visit Without Timer";
        public string Description { get; set; } = "Task Visit Without Timer Description";
        public double Pay { get; set; } = 0;
        public int CountViewsLeft { get; set; } = 0;
        public string taskLink { get; set; } = "";
        public string taskConfirmationLink { get; set; } = "";
    }
}
