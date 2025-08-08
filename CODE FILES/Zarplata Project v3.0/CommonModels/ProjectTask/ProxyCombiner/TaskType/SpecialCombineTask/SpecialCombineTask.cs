using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.SpecialCombineTask.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.ProjectTask.ProjectTaskEnums;

namespace CommonModels.ProjectTask.ProxyCombiner.TaskType.SpecialCombineTask
{
    public class SpecialCombineTask : ProxyCombinerTask
    {
        public ParseRequirements parseRequirements { get; set; }
        public CheckRequirements checkRequirements { get; set; }
        public SpecialRequirements specialRequirements { get; set; }
        public CheckedProxy? result { get; set; } = null;

        public SpecialCombineTask(string authorId, ProjectTaskEnums.TaskFrom assignedBy, ProjectTaskEnums.TaskStatus status, ParseRequirements parseRequirements, CheckRequirements checkRequirements, SpecialRequirements specialRequirements, TaskActionAfterFinish actionAfterFinish = TaskActionAfterFinish.Remove) : base(authorId, assignedBy, status, actionAfterFinish)
        {
            base.InternalType = ProxyCombinerTaskEnums.ProxyCombinerTaskType.SpecialCombine;

            this.parseRequirements = parseRequirements;
            this.checkRequirements = checkRequirements;
            this.specialRequirements = specialRequirements;
        }
    }
}
