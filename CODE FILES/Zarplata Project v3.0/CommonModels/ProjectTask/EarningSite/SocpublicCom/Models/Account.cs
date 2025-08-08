
using CommonModels.ProjectTask.Platform.SocpublicCom.Models.AccountInternalModels;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using CommonModels.UserAgentClasses;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Net;
using CommonModels.ProjectTask.ProxyCombiner.Models;

namespace CommonModels.ProjectTask.Platform.SocpublicCom.Models
{
    public class Account
    {
        [BsonId]
        [BsonElement("Id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null;
        public string? BotPatternId { get; set; } = null;
        public string? Login { get; set; } = null;
        public EmailModels.Email? Email { get; set; } = null;
        public bool? EmailIsConfirmed { get; set; } = null;
        public string? Phone { get; set; } = null;
        public string? Password { get; set; } = null;
        public string? Pincode { get; set; } = null;
        public bool? IsAlive { get; set; } = null;
        public bool? IsMain { get; set; } = null;
        public CheckedProxy? PersonalProxy { get; set; } = null;
        public EnvironmentProxy? PersonalPrivateProxy { get; set; } = null;
        public bool IsFirstExecutionOfTasksAfterRegistration { get; set; } = true;
        public double? MoneyMainBalance { get; set; } = null;
        public double? MoneyAdvertisingBalance { get; set; } = null;
        public Referal? Refer { get; set; } = null;
        public Finance? Finance { get; set; } = null;
        public Security? Security { get; set; } = null;
        public Privacy? Privacy { get; set; } = null;
        public Notifications? Notifications { get; set; } = null;
        public Anketa? Anketa { get; set; } = null;
        public CareerLadder? CareerLadder { get; set; } = null; // set on server side from history
        public DateTime? RegedDateTime { get; set; } = null;
        public CheckedProxy? RegedProxy { get; set; } = null;
        public UserAgent? RegedUseragent { get; set; } = null;
        public Dictionary<string, List<string>>? RegedHeaders { get; set; } = null;
        public List<Referal>? Referals { get; set; } = null;
        public Dictionary<string, List<string>>? LastHeaders { get; set; } = null; // set on server side from history
        public List<Cookie>? LastCookies { get; set; } = null; // set on server side from history
        public List<OneLogAccounResult>? History { get; set; } = null;
    }
}
