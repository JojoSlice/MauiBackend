using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MauiBackend.Models
{
    public class Asset
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Ticker { get; set; }
        public string Period { get; set; } = "1H";
    }
}
