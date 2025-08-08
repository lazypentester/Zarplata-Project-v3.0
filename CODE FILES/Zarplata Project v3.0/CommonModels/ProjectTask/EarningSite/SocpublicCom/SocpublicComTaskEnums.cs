using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.Platform.SocpublicCom
{
    public class SocpublicComTaskEnums
    {
        public enum SocpublicComTaskType
        {
            GroupSelectiveTaskWithAuth,
            GroupSelectiveTaskWithoutAuth,
            AutoregTask
        }

        public enum SocpublicComTaskSubtype
        {
            TaskVisitWithoutTimer,
            TaskVisitWithTimer,
            CheckCareerLadder,
            CheckOnNeedToConfirmAccountEmail,
            ConfirmRegisterAccountByLinkFromEmail,
            WithdrawalOfMoney,
            CheckAndGetFreeGiftBoxes,
            UpdatingTheCurrentBalance,
            PassRulesKnowledgeTest,
            FillAnketaMainInfo,
            InstallAvatar
        }
    }
}
