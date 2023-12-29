using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EphemeralEnvironments.Service.Models
{
    [BsonIgnoreExtraElements]
    public class Document
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("payload")]
        public Payload Payload { get; set; }
    }
}
