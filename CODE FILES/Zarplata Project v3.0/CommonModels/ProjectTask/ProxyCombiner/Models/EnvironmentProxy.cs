using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CommonModels.ProjectTask.ProxyCombiner.Models
{
    public class EnvironmentProxy
    {
        //[BsonId]
        //[BsonElement("Id")]
        //[BsonRepresentation(BsonType.ObjectId)]
        //public string? Id { get; set; } = null;
        public string? Name { get; set; } = null;
        public string? Description { get; set; } = null;
        public EPMarker? Marker { get; set; } = null;
        public string? Address { get; set; } = null;
        public string? Port { get; set; } = null;
        public string? Username { get; set; } = null;
        public string? Password { get; set; } = null;
        public EPProtocol? Protocol { get; set; } = null;
        public string? FullProxyAddress
        {
            get
            {
                return $"{Protocol}://{Address}:{Port}";
            }
        }
        public Uri? FullProxyAddressUri
        {
            get
            {
                return new Uri($"{Protocol}://{Address}:{Port}");
            }
        }

        //public EnvironmentProxy(string name, string description, EPMarker marker, string address, string port, EPProtocol protocol)
        //{
        //    Name = name;
        //    Description = description;
        //    Marker = marker;
        //    Address = address;
        //    Port = port;
        //    Protocol = protocol;
        //}

        public enum EPProtocol
        {
            http,
            socks4,
            socks5
        }

        public enum EPMarker
        {
            Parser,
            MainAccount,
            Other
        }
    }
}
