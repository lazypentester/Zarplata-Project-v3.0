using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.ProjectTask.ProjectTaskEnums;

namespace CommonModels.ProjectTask.ProxyCombiner.TaskType.DefaultCombineTask
{
    public class DefaultCombineTask : ProxyCombinerTask
    {
        public ParseRequirements parseRequirements { get; set; }
        public CheckRequirements checkRequirements { get; set; }
        public List<CheckedProxy>? result { get; set; } = null;

        public DefaultCombineTask(string authorId, ProjectTaskEnums.TaskFrom assignedBy, ProjectTaskEnums.TaskStatus status, ParseRequirements parseRequirements, CheckRequirements checkRequirements, TaskActionAfterFinish actionAfterFinish = TaskActionAfterFinish.Remove) : base(authorId, assignedBy, status, actionAfterFinish)
        {
            base.InternalType = ProxyCombinerTaskEnums.ProxyCombinerTaskType.DefaultCombine;

            this.parseRequirements = parseRequirements;
            this.checkRequirements = checkRequirements;
        }
    }
}
