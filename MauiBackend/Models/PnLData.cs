using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MauiBackend.Models
{
    public class PnLData
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        public double PnL { get; set; }
        public DateTime Date { get; set; }
    }
}
