using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels
{
    public class Notifications
    {
        bool? EnableEmailNotifications { get; set; } = false;
        bool? MessagesFromAdmin { get; set; } = false;
        bool? MessagesFromRefer { get; set; } = false;
        bool? EventNotifications { get; set; } = false;
        bool? TenMoreTaskRequests { get; set; } = false;
    }
}
