using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.ProjectTask.ProjectTaskEnums;

namespace CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask
{
    public class CheckTask : ProxyCombinerTask
    {
        public List<ParsedProxy> parsedProxyList { get; set; }
        public CheckRequirements checkRequirements { get; set; }
        public List<CheckedProxy>? result { get; set; } = null;

        public CheckTask(string authorId, ProjectTaskEnums.TaskFrom assignedBy, ProjectTaskEnums.TaskStatus status, List<ParsedProxy> parsedProxyList, CheckRequirements checkRequirements, TaskActionAfterFinish actionAfterFinish = TaskActionAfterFinish.Remove) : base(authorId, assignedBy, status, actionAfterFinish)
        {
            base.InternalType = ProxyCombinerTaskEnums.ProxyCombinerTaskType.Check;

            this.parsedProxyList = parsedProxyList;
            this.checkRequirements = checkRequirements;
        }
    }
}
