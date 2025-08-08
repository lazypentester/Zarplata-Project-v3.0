using CommonModels.ProjectTask.EarningSite.SocpublicCom.Models.ConfirmWithdrawalOfMoneyModels;
using CommonModels.ProjectTask.Platform.SocpublicCom;
using CommonModels.ProjectTask.Platform.SocpublicCom.Models;
using CommonModels.ProjectTask.Platform.SocpublicCom.TaskType.SelectiveTaskWithAuth;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.ProjectTask.Platform.SocpublicCom.SocpublicComTaskEnums;

namespace CommonModels.ProjectTask.EarningSite.SocpublicCom.TaskType.GSelectiveTaskWithAuth.SelectiveTask.WithdrawMoney
{
    [BsonDiscriminator("WithdrawMoneyGroupSelectiveTaskWithAuth")]
    public class WithdrawMoneyGroupSelectiveTaskWithAuth : SocpublicComTask
    {
        public SocpublicComTaskSubtype[]? InternalSubtype { get; set; } = null;
        public string? IdOfTheBotAttachedToThisMainAccount { get; set; } = null;
        public string? DatabaseSlaveAccountId { get; set; } = null;
        public string? DatabaseMainAccountId { get; set; } = null;
        public string? DatabaseSlaveAccountLogin { get; set; } = null;
        public string? DatabaseMainAccountLogin { get; set; } = null;
        public bool? IsTaskCreatedOnPlatformBySlave { get; set; } = null;
        public bool? IsTaskCompletedOnPlatformByMain { get; set; } = null;
        public WithdrawalInternalStatus? InternalStatus { get; set; } = null;
        public WithdrawalOfMoneyPlatformTaskModel? WithdrawalOfMoneyPlatformTask { get; set; } = null;
        public bool? NeedToEditPlatformTaskForSlave { get; set; } = null; // это поле на случай когда нужно будет обновить данные задания опираясь на паттерн из бд
        public bool? NeedToRemovePlatformTaskForSlave { get; set; } = null; // это поле на случай когда нужно будет обновить данные задания опираясь на паттерн из бд
        public WithdrawMoneyGroupSelectiveTaskWithAuth(string authorId, ProjectTaskEnums.TaskFrom assignedBy, string url, string databaseAccountId, Account account, SocpublicComTaskEnums.SocpublicComTaskSubtype[] internalSubtype, ProjectTaskEnums.TaskStatus status = ProjectTaskEnums.TaskStatus.Created, ProjectTaskEnums.TaskActionAfterFinish actionAfterFinish = ProjectTaskEnums.TaskActionAfterFinish.Renew) : base(authorId, assignedBy, status, url, databaseAccountId, account, actionAfterFinish)
        {
            base.InternalType = SocpublicComTaskType.GroupSelectiveTaskWithAuth;
            InternalSubtype = internalSubtype;
            InternalStatus = WithdrawalInternalStatus.Created;
        }

        public enum WithdrawalInternalStatus
        {
            Created,
            StartedBySlave,
            CompletedSuccessfullBySlave,
            CompletedWithErrorBySlave,
            StartedByMain,
            CompletedSuccessfullyByMain,
            CompletedWithErrorByMain,
        }
    }
}
