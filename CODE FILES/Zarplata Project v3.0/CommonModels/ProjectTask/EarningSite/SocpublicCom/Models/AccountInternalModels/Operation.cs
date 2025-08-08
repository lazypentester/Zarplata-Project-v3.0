

namespace CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels
{
    public class Operation
    {
        public DateTime? DateTimeStart { get; set; } = null;
        public DateTime? DateTimeEnd { get; set; } = null;
        public double? TimeOfWorkInSeconds { get; set; } = null;
        public OperationType? Type { get; set; } = null;
        public OperationStatus? Status { get; set; } = null;
        public string? StatusMessage { get; set; } = null;
        public double? MoneyMainBalancePlus { get; set; } = null;
        public double? MoneyMainBalanceMinus { get; set; } = null;
        public double? MoneyAdvertisingBalancePlus { get; set; } = null;
        public double? MoneyAdvertisingBalanceMinus { get; set; } = null;
        public string? Action { get; set; } = null;

        public void SetTimeOfWorkInSeconds()
        {
            TimeOfWorkInSeconds = DateTimeEnd!.Value.Subtract((DateTime)DateTimeStart!).TotalSeconds;
        }
    }

    public enum OperationType
    {
        executeOneTask,
        executeTasks,
        checkCareerLadder,
        receivETrainingBeforePerformingVisitsWithoutTimer,
        withdrawalOfMoneyByMain,
        withdrawalOfMoneyBySlave,
        checkAndGetFreeGiftBoxes,
        updatingTheCurrentBalance,
        passRulesKnowledgeTest,
        fillAnketaMainInfo,
        installAvatar
    }

    public enum OperationStatus
    {
        Success,
        Failure
    }
}
