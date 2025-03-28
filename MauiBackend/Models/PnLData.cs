﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MauiBackend.Models
{
    public class PnLData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string  id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;
        [BsonRepresentation(BsonType.ObjectId)]
        public string SeasonId { get; set; }
        public double PnL { get; set; } = 0;
        public DateTime Date { get; set; }
    }
}
