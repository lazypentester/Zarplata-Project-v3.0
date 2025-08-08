using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.Client.Models
{
    public class ModelBlockedMachine
    {
        [BsonId]
        [BsonElement("Id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ID { get; set; } = null;

        [BsonElement("Ip")]
        public string? IP { get; set; } = null!;

        [BsonElement("Machine")]
        public Machine MACHINE { get; set; } = null!;
    }
}
