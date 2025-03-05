using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MauiBackend.Models
{
    public class Season
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SeasonMessage { get; set; } = string.Empty;
    }
}
