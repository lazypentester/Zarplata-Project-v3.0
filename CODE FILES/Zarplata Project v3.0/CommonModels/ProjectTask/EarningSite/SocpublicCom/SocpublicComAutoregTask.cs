using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums;
using CommonModels.EmailModels;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using static CommonModels.ProjectTask.EarningSite.EarningSiteTaskEnums;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using TaskStatus = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus;

namespace CommonModels.ProjectTask.EarningSite.SocpublicCom
{
    public class SocpublicComAutoregTask : EarningSiteTask
    {
        public SocpublicComTaskType? InternalType { get; set; } = null;
        public EmailModels.Email Email { get; set; }
        public Account? Account { get; set; } = null;

        public SocpublicComAutoregTask(string authorId, TaskFrom assignedBy, TaskStatus status, string url, EmailModels.Email email, TaskActionAfterFinish actionAfterFinish) : base(authorId, assignedBy, status, url, actionAfterFinish)
        {
            this.Email = email;

            base.Site = EarningSiteEnum.SocpublicDotCom;

            InternalType = SocpublicComTaskType.AutoregTask;
        }
    }
}
