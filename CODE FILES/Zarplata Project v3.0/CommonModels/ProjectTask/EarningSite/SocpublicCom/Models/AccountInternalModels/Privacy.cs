using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels
{
    public class Privacy
    {
        bool? GlobalContests { get; set; } = false;
        bool? CompetitionsFromRefer { get; set; } = false;
        bool? TOPForEarnings { get; set; } = false;
        bool? TOPForReferrals { get; set; } = false;
        bool? TOPForAdvertising { get; set; } = false;
        bool? OtherTOPs { get; set; } = false;
        bool? StatisticOnEarnings { get; set; } = false;
        bool? AdvertisingStatistics { get; set; } = false;
        bool? ReferralStatistics { get; set; } = false;
        bool? Achievements { get; set; } = false;
    }
}
