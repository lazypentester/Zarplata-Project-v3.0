using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.EarningSite.SearchTasksModels
{
    public class SearchTaskEnums
    {
        public enum FindSearchKeywordParametersEarnSiteTasks
        {
            Id,
            AuthorId,
            ExecutorId,
            DateTimeCreate,
            DateTimeStart,
            DateTimeEnd,
            ErrorMessage
        }

        public enum FindFilterParametersEarnSiteTasks
        {
            Type,
            AssignedBy,
            Status,
            ResultStatus,
            ErrorStatus,
            ActionAfterFinish
        }

        public enum FindSortEarnSiteTasks
        {
            NewFirst,
            OldFirst
        }
    }
}
