using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using TaskStatus = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus;

namespace CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithoutAuth
{
    public class GroupSelectiveTaskWithoutAuth : SocpublicComTask
    {
        public SocpublicComTaskSubtype[] InternalSubtype { get; set; }
        public GroupSelectiveTaskWithoutAuth(string authorId, TaskFrom assignedBy, string url, string databaseAccountId, Account account, SocpublicComTaskSubtype[] internalSubtype, TaskStatus status = TaskStatus.Created, TaskActionAfterFinish actionAfterFinish = TaskActionAfterFinish.Renew) : base(authorId, assignedBy, status, url, databaseAccountId, account, actionAfterFinish)
        {
            base.InternalType = SocpublicComTaskType.GroupSelectiveTaskWithoutAuth;
            InternalSubtype = internalSubtype;
        }
    }
}
