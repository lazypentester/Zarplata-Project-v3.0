using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels
{
    public class DailyBonus
    {
        public bool? Bonus { get; set; } = null;
        public bool? Error { get; set; } = null;
        public double? StatusPoints { get; set; } = null;
        public double? MoneyMainBalancePlus { get; set; } = null;
    }
}
