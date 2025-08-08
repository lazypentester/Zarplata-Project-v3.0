using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using static CommonModels.Client.Client;

namespace CommonModels.Client.Models
{
    public class ModelClient
    {
        [BsonId]
        [BsonElement("Id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ID { get; set; } = null;
        [BsonElement("ServerHost")]
        public string? SERVER_HOST { get; set; } = null!;
        [BsonElement("RegistrationDateTime")]
        public DateTime? RegistrationDateTime { get; set; } = null!;
        [BsonElement("DisconnectDateTime")]
        public DateTime? DisconnectDateTime { get; set; } = null!;
        [BsonElement("Role")]
        public BotRole? Role { get; set; } = null!;
        [BsonElement("Ip")]
        public string? IP { get; set; } = null!;
        [BsonElement("Machine")]
        public Machine MACHINE { get; set; } = null!;
        [BsonElement("Status")]
        public ClientStatus Status { get; set; } = ClientStatus.Started;
    }
}
