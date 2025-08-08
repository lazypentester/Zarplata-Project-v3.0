using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels
{
    public class CareerLadder
    {
        public Status? Status { get; set; } = null;
        public double? StatusPoints { get; set; } = null;
        public int? StatusProgress { get; set; } = null;
        public int? CareerLevel { get; set; } = null;
    }

    public enum Status
    {
        beginner = 1,
        student = 2,
        experienced = 3,
        advanced = 4,
        activist = 5,
        specialist = 6,
        expert = 7,
        master = 8,
        grandmaster = 9
    }
}
