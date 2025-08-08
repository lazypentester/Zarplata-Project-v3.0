using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using static CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums;
using static CommonModels.ProjectTask.ProjectTaskEnums;
using TaskStatus = CommonModels.ProjectTask.ProjectTaskEnums.TaskStatus;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth.SelectiveTask.TaskVisitWithoutTimer;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.GroupSelectiveTaskWithAuth.SelectiveTask.CheckCareerLadder;
using CommonModels.ProjectTask.EarningSite.SocpublicCom;
using MongoDB.Bson.Serialization.Attributes;

namespace CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.SelectiveTaskWithAuth
{
    [BsonDiscriminator("GroupSelectiveTaskWithAuth")]
    public class GroupSelectiveTaskWithAuth : SocpublicComTask
    {
        public SocpublicComTaskSubtype[] InternalSubtype { get; set; }
        public TaskVisitWithoutTimer? taskVisitWithoutTimer { get; set; } = null;
        public CheckCareerLadder? checkCareerLadder { get; set; } = null;
        public GroupSelectiveTaskWithAuth(string authorId, TaskFrom assignedBy, string url, string databaseAccountId, Account account, SocpublicComTaskSubtype[] internalSubtype, TaskStatus status = TaskStatus.Created, TaskActionAfterFinish actionAfterFinish = TaskActionAfterFinish.Renew) : base(authorId, assignedBy, status, url, databaseAccountId, account, actionAfterFinish)
        {
            base.InternalType = SocpublicComTaskType.GroupSelectiveTaskWithAuth;
            InternalSubtype = internalSubtype;
        }
    }
}
