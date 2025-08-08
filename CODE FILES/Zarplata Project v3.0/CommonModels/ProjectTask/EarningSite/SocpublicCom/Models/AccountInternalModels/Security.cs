using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels
{
    public class Security
    {
        bool? CheckIp { get; set; } = false;    
        bool? IPProtection { get; set; } = false;    
        bool? UserAgentProtection { get; set; } = false;    
    }
}
