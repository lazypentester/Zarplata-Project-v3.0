using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Client.Models.SearchBotsModels
{
    public class SearchBotEnums
    {
        public enum FindSearchKeywordParametersBots
        {
            Id,
            Ip,
            MachineName,
            UserName,
            RegistrationDateTime,
            OSPlatformText,
            IdentityKey
        }

        public enum FindFilterParametersBots
        {
            Role,
            OSPlatform,
            Status
        }

        public enum FindSortBots
        {
            NewFirst,
            OldFirst
        }
    }
}
