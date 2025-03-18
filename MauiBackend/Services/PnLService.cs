using MauiBackend.Models;
using MongoDB.Driver;

namespace MauiBackend.Services
{
    public class PnLService
    {
        private readonly IMongoCollection<PnLData> _pnlDataCollection;
        private readonly MongoDbService _mongoDbService;
        public PnLService(IConfiguration config, MongoDbService mongoDbService)
        {

            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:Database"]);
            _pnlDataCollection = database.GetCollection<PnLData>("PnLData");
            _mongoDbService = mongoDbService;
        }

        public async Task<List<PnLData>> GetPnLAsync(string userId)
        {
            Console.WriteLine("PnL hämtas");
            var season = await _mongoDbService.GetCurrentSeason();
            if (season == null)
            {
                season = new Season
                {
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow.Date.AddDays(30),
                    Name = "Season 0",
                    SeasonMessage = "Dev season"
                };
                await _mongoDbService.CreateSeason(season);
            }

            var filter = Builders<PnLData>.Filter.And(
                Builders<PnLData>.Filter.Eq(pnl => pnl.UserId, userId),
                Builders<PnLData>.Filter.Eq(pnl => pnl.SeasonId, season.Id));

            List<PnLData> pnls = new();

            pnls = await _pnlDataCollection.Find(filter).ToListAsync();

            
            var latestPnl = await GetLatestPnL(userId);
            
            if ( latestPnl == null || latestPnl.Date.Date != DateTime.UtcNow.Date )
            {
                var newPnl = new PnLData
                {
                    UserId = userId,
                    SeasonId = season.Id,
                    PnL = latestPnl?.PnL ?? 0,
                    Date = DateTime.UtcNow
                };

                pnls.Add(newPnl);
                await _pnlDataCollection.InsertOneAsync(newPnl);
            }

            Console.WriteLine("GetPnL slut");
            return pnls;
        }

        public async Task<PnLData> GetLatestPnL(string userId)
        {
            var season = await _mongoDbService.GetCurrentSeason();

            var filter = Builders<PnLData>.Filter.And(
                Builders<PnLData>.Filter.Eq(pnl => pnl.UserId, userId),
                Builders<PnLData>.Filter.Eq(pnl => pnl.SeasonId, season.Id));

            List<PnLData> pnls = new();

            pnls = await _pnlDataCollection.Find(filter).ToListAsync();
            var today = DateTime.UtcNow.Date;

            
            return pnls.OrderByDescending(p => p.Date).FirstOrDefault();
        }

        public async Task UpdatePnL(TradeData trade)
        {
            var today = DateTime.UtcNow.Date;
            var filter = Builders<PnLData>.Filter.And(
                Builders<PnLData>.Filter.Eq(p => p.UserId, trade.UserId),
                Builders<PnLData>.Filter.Eq(p => p.Date.Date, today));

            var pnl = await _pnlDataCollection.Find(filter).FirstOrDefaultAsync();
            if (pnl != null)
            {
                var update = Builders<PnLData>.Update.Inc(p => p.PnL, trade.PnLPercent.Value);

                await _pnlDataCollection.UpdateOneAsync(filter, update);
            }
            else
            {
                var latestPnl = await GetLatestPnL(trade.UserId);

                var newPnL = new PnLData
                {
                    UserId = trade.UserId,
                    Date = today,
                    PnL = latestPnl.PnL += trade.PnLPercent.Value
                };

                
                await _pnlDataCollection.InsertOneAsync(newPnL);
            }
        }
    }
}
