using CommonModels.ProjectTask.ProxyCombiner.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.ProjectTask.ProjectTaskEnums;

namespace CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask
{
    public class ParseTask : ProxyCombinerTask
    {
        public ParseRequirements parseRequirements { get; set; }
        public List<ParsedProxy>? result { get; set; } = null;

        public ParseTask(string authorId, ProjectTaskEnums.TaskFrom assignedBy, ProjectTaskEnums.TaskStatus status, ParseRequirements parseRequirements, TaskActionAfterFinish actionAfterFinish = TaskActionAfterFinish.Remove) : base(authorId, assignedBy, status, actionAfterFinish)
        {
            base.InternalType = ProxyCombinerTaskEnums.ProxyCombinerTaskType.Parse;

            this.parseRequirements = parseRequirements;
        }
    }
}
