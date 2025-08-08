using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.User.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonElement("Id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null;
        [BsonElement("Username")]
        public string? Username { get; set; } = null;
        [BsonElement("Password")]
        public UserPasswordModel? Password { get; set; } = null;
        [BsonElement("Roles")]
        public UserRole[]? Roles { get; set; } = null;
        [BsonElement("RegistrationDateTime")]
        public DateTime? RegistrationDateTime { get; set; } = null!;
    }

    public enum UserRole
    {
        Admin,
        Spectator,
        NotAuthenticatedUser
    }

    public enum UserActions
    {
        CanLookBots,
        CanCopyBots,
        CanCheckDetailsBots,
        CanStopOrStartBots,
        CanDeleteBots,
        CanBlockBots,

        CanLookEarnSiteTasks,
        CanCopyEarnSiteTasks,
        CanCheckDetailsEarnSiteTasks,
        CanDeleteEarnSiteTasks
    }
}
