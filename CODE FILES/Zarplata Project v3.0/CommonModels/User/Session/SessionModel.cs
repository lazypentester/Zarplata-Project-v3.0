using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.User.Session
{
    public class SessionModel
    {
        [BsonId]
        [BsonElement("Id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null;
        [BsonElement("UserId")]
        public string? UserId { get; set; } = null;
        [BsonElement("StartDateTime")]
        public DateTime? StartDateTime { get; set; } = null;
        [BsonElement("ExpiresDateTime")]
        public DateTime? ExpiresDateTime { get; set; } = null;
        [BsonElement("RefreshToken")]
        public string? RefreshToken { get; set; } = null;
        [BsonElement("Fingerprint")]
        public string? Fingerprint { get; set; } = null;
        [BsonElement("Ip")]
        public string? Ip { get; set; } = null;
    }
}
