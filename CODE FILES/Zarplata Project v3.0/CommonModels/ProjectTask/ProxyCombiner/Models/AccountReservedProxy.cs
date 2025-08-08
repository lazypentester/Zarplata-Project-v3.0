using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using CommonModels.ProjectTask.Platform;
using static CommonModels.ProjectTask.EarningSite.EarningSiteTaskEnums;

namespace CommonModels.ProjectTask.ProxyCombiner.Models
{
    public class AccountReservedProxy
    {
        [BsonId]
        [BsonElement("Id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null;
        public string AccountId { get; set; }
        public EarningSiteEnum ReservedPlatform { get; set; }
        public DateTime ReservedDateTime { get; set; }
        public ParsedProxy Proxy { get; set; }

        public AccountReservedProxy(string accountId, EarningSiteEnum reservedPlatform, DateTime reservedDateTime, ParsedProxy proxy)
        {
            this.AccountId = accountId;
            this.ReservedPlatform = reservedPlatform;
            this.ReservedDateTime = reservedDateTime;
            this.Proxy = proxy;
        }
    }
}
