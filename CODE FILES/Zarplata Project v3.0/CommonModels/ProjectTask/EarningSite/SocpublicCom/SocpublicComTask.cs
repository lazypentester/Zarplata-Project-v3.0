using static CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums;
using TaskStatus = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using static CommonModels.ProjectTask.EarningSite.EarningSiteTaskEnums;
using MongoDB.Bson.Serialization.Attributes;

namespace CommonModels.ProjectTask.EarningSite.SocpublicCom
{
    public abstract class SocpublicComTask : EarningSiteTask
    {
        public SocpublicComTaskType InternalType { get; set; }
        public string? DatabaseAccountId { get; set; } = null;
        public Account? Account { get; set; } = null;

        public SocpublicComTask(string authorId, TaskFrom assignedBy, TaskStatus status, string url, TaskActionAfterFinish actionAfterFinish) : base(authorId, assignedBy, status, url, actionAfterFinish)
        {
            base.Site = EarningSiteEnum.SocpublicDotCom;
        }

        protected SocpublicComTask(string authorId, TaskFrom assignedBy, TaskStatus status, string url, string databaseAccountId, Account account, TaskActionAfterFinish actionAfterFinish) : base(authorId, assignedBy, status, url, actionAfterFinish)
        {
            DatabaseAccountId = databaseAccountId;
            Account = account;

            base.Site = EarningSiteEnum.SocpublicDotCom;
        }
    }
}
