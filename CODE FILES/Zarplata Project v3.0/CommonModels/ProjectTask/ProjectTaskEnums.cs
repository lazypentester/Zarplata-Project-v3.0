using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask
{
    public class ProjectTaskEnums
    {
        public enum TaskType
        {
            EarningSiteWork,
            ProxyCombineWork,
            AutoregWork,
            ManageWork
        }

        public enum TaskStatus
        {
            Created,
            Started,
            WaitingForCheckAccountHistoryProxy,
            WaitingForNewProxy,
            WaitingForCaptcha,
            Executing,
            Done
        }

        public enum TaskResultStatus
        {
            Unknown,
            Success,
            Error
        }

        public enum TaskErrorStatus
        {
            CaptchaError,
            ConnectionError,
            AnotherError
        }

        public enum TaskFrom
        {
            Bot,
            Management,
        }

        public enum TaskActionAfterFinish
        {
            Renew,
            Remove
        }
    }
}
