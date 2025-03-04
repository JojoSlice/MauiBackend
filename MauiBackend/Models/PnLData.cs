using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MauiBackend.Models
{
    public class PnLData
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;
        public double PnLPercent { get; set; } = 0;
        public double Points { get; set; } = 1000;
        public DateTime Date { get; set; }
    }
}
