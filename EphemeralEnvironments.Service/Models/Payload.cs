using MongoDB.Bson.Serialization.Attributes;

namespace EphemeralEnvironments.Service.Models
{
    [BsonIgnoreExtraElements]
    public class Payload
    {
        [BsonElement("type")]
        public string Type { get; set; }
        [BsonElement("value")]
        public string Value { get; set; }
        [BsonElement("timeAdded")]
        public DateTime TimeAdded { get; set; }
    }
}
