using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.ProjectTask
{
    public class RunTasksSettings
    {
        [BsonId]
        [BsonElement("Id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null;
        public bool? TasksIsRunning { get; set; } = null;
        public LaunchTaskType? CurrentLaunchedTaskType { get; set; } = null;
        public bool? AutoRelaunchTasksQueue { get; set; } = null;
        public bool? AutoRelaunchTaskIfError { get; set; } = null;
        public int? MaxErrorForTaskCount { get; set; } = null;
    }

    public enum LaunchTaskType
    {
        EarningMoney,
        AutoregAccounts,
        WithdrawingMoney
    }
}
