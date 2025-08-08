using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.EarningSite.SocpublicCom.Models.ConfirmWithdrawalOfMoneyModels
{
    public class WithdrawalOfMoneyPlatformTaskModel
    {
        public string? PlatformTaskId { get; set; } = null;
        public string? PlatformTask_Bid_Id { get; set; } = null;
        public bool? AutoPayment { get; set; } = null;
        public int? PlatformTaskExecuteMaxTimeInSecs { get; set; } = null;
        public string? PlatformTaskName { get; set; } = null;
        public double? PlatformTaskMoney { get; set; } = null;
        public string? PlatformTaskCodePasswordForSolveTask { get; set; } = null;
        public string? PlatformTaskTextForSolveTask { get; set; } = null;
        public DateTime? LastCheckTime { get; set; } = null;
    }
}
