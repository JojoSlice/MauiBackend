using MauiBackend.Models;
using MongoDB.Driver;

namespace MauiBackend.Services
{
    public class PnLService
    {
        private readonly IMongoCollection<PnLData> _pnlDataCollection;
        private readonly MongoDbService _mongoDbService;
        private readonly TradeDataService _tradeDataService;
        public PnLService(IConfiguration config, MongoDbService mongoDbService, TradeDataService tradeDataService)
        {

            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:Database"]);
            _pnlDataCollection = database.GetCollection<PnLData>("PnLData");
            _mongoDbService = mongoDbService;
            _tradeDataService = tradeDataService;
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
            await UpdatePnLAsync(userId);

            pnls = await _pnlDataCollection.Find(filter).ToListAsync();

            if (pnls.Count == 0)
            {
                var newPnl = new PnLData
                {
                    UserId = userId,
                    SeasonId = season.Id,
                    Date = DateTime.UtcNow
                };

                pnls.Add(newPnl);
                await _pnlDataCollection.InsertOneAsync(newPnl);
            }

            Console.WriteLine("GetPnL slut");
            return pnls;
        }


        public async Task UpdatePnLAsync(string id)
        {
            var pnl = new List<PnLData>();

            var allTrades = await _tradeDataService.GetClosedTradesByUserIdAsync(id);

            if (allTrades.Any())
            {
                var tradesByDate = allTrades.GroupBy(td => td.TradeDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalPnLPercent = g.Sum(t => t.PnLPercent)
                    })
                    .OrderBy(g => g.Date)
                    .ToList();

                foreach (var tradDate in tradesByDate)
                {
                    pnl.Add(new PnLData
                    {
                        UserId = id,
                        Date = tradDate.Date,
                        PnL = tradDate.TotalPnLPercent ?? 0
                    });
                }
            }

            var filter = Builders<PnLData>.Filter.Eq(p => p.UserId, id);


            if (pnl.Any())
            {
                await _pnlDataCollection.DeleteManyAsync(filter);
                await _pnlDataCollection.InsertManyAsync(pnl);
            }
        }

    }
}
