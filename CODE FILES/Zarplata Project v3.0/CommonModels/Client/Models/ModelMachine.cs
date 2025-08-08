using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonModels.Client.Machine;

namespace CommonModels.Client.Models
{
    public class ModelMachine
    {
        [BsonElement("MachineName")]
        [Required(ErrorMessage = "MACHINE_NAME is null")]
        public string? MACHINE_NAME { get; set; } = null!;

        [BsonElement("OSPlatform")]
        [Required(ErrorMessage = "OS_PLATFORM is null")]
        public Platform? OS_PLATFORM { get; set; } = null;

        [BsonElement("OSPlatformText")]
        [Required(ErrorMessage = "OS_PLATFORM_TEXT is null")]
        public string? OS_PLATFORM_TEXT { get; set; } = null;

        [BsonElement("UserName")]
        [Required(ErrorMessage = "USER_NAME is null")]
        public string? USER_NAME { get; set; } = null!;

        [Required(ErrorMessage = "KEY is null")]
        [BsonElement("IdentityKey")]
        public string? IDENTITY_KEY { get; set; } = null!;
    }
}
