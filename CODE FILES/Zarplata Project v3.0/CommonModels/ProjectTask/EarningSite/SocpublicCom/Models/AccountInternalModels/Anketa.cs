using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels
{
    public class Anketa
    {
        public bool? AvatarInstalled { get; set; } = null;
        public string? AvatarUrl { get; set; } = null;
        public string? AvatarInfo { get; set; } = null;
        public bool? AnketaMainInfoIsFilled { get; set; } = null;
        public KindOfActivity? KindOfActivity { get; set; } = null;
        public FamilyStatus? FamilyStatus { get; set; } = null;
        public Gender? Gender { get; set; } = null;
        public DateTime? DateOfBirth { get; set; } = null;
        public bool? TestPassed { get; set; } = null;
    }

    public enum KindOfActivity
    {
        school = 0,
        high = 1,
        gray = 2,
        white = 3,
        business_real = 4,
        business_it = 5,
        no = 6,
    }

    public enum FamilyStatus
    {
        no = 0,
        no_child = 1,
        yes = 2,
        yes_child = 3
    }

    public enum Gender
    {
        male = 0,
        female = 1
    }
}
