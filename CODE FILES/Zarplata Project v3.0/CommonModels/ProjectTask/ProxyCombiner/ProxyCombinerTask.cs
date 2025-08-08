using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonModels.ProjectTask;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using static CommonModels.ProjectTask.ProxyCombiner.ProxyCombinerTaskEnums;

namespace CommonModels.ProjectTask.ProxyCombiner
{
    public class ProxyCombinerTask : ProjectTask
    {
        public ProxyCombinerTaskType InternalType { get; set; }

        public ProxyCombinerTask(string authorId, ProjectTaskEnums.TaskFrom assignedBy, ProjectTaskEnums.TaskStatus status, TaskActionAfterFinish actionAfterFinish) : base(authorId, assignedBy, status, actionAfterFinish)
        {
            base.Type = ProjectTaskEnums.TaskType.ProxyCombineWork;
        }

        public ProxyCombinerTask(string authorId, string executorId, ProjectTaskEnums.TaskFrom assignedBy, ProjectTaskEnums.TaskStatus status, TaskActionAfterFinish actionAfterFinish) : base(authorId, executorId, assignedBy, status, actionAfterFinish)
        {
            base.Type = ProjectTaskEnums.TaskType.ProxyCombineWork;
        }
    }
}
