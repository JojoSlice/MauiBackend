using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace MauiBackend.Models
{
    public class TradeData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public string Exchange { get; set; } = string.Empty;

        private string _ticker = string.Empty;
        public string Ticker
        { 
            get => _ticker;
            set => _ticker = value.ToUpper();
        }
        public double Price { get; set; }
        public double Points { get; set; }
        public bool IsLong { get; set; }

        public DateTime TradeDate { get; set; } = DateTime.UtcNow;
        public bool IsOpen { get; set; } = true;

        public double? StopLoss { get; set; }
        public double? TakeProfit { get; set; }

        public double? PnLPercent { get; set; }
        public double? ClosingPrice { get; set; }

    }
}
