using TaskFrom = CommonModels.ProjectTask.ProjectTaskEnums.TaskFrom;
using TaskType = CommonModels.ProjectTask.ProjectTaskEnums.TaskType;
using TaskStatus = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus;
using System;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using static CommonModels.ProjectTask.EarningSite.EarningSiteTaskEnums;

namespace CommonModels.ProjectTask.EarningSite
{
    public abstract class EarningSiteTask : ProjectTask
    {
        public EarningSiteEnum Site { get; set; }
        public string Url { get; set; }

        public EarningSiteTask(string authorId, TaskFrom assignedBy, TaskStatus status, string url, TaskActionAfterFinish actionAfterFinish) : base(authorId, assignedBy, status, actionAfterFinish)
        {
            Url = url;

            base.Type = TaskType.EarningSiteWork;
        }

        public EarningSiteTask(string authorId, string executorId, TaskFrom assignedBy, TaskStatus status, string url, TaskActionAfterFinish actionAfterFinish) : base(authorId, executorId, assignedBy, status, actionAfterFinish)
        {
            Url = url;

            base.Type = TaskType.EarningSiteWork;
        }
    }
}
