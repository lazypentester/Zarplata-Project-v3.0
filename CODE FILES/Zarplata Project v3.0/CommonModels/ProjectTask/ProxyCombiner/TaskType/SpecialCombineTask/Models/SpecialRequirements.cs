using CommonModels.ProjectTask.EarningSite;
using CommonModels.ProjectTask.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.ProxyCombiner.TaskType.SpecialCombineTask.Models
{
    public class SpecialRequirements
    {
        public string? PlatformAccountId { get; set; }
        public EarningSiteTaskEnums.EarningSiteEnum ReservedPlatform { get; set; }

        public SpecialRequirements(string? platformAccountId, EarningSiteTaskEnums.EarningSiteEnum reservedPlatform)
        {
            PlatformAccountId = platformAccountId;
            ReservedPlatform = reservedPlatform;
        }
    }
}
