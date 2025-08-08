using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.User.Models
{
    public class UserPasswordModel
    {
        [BsonElement("Salt")]
        public string? Salt { get; set; } = null;
        [BsonElement("Hash")]
        public string? Hash { get; set; } = null;
    }
}
