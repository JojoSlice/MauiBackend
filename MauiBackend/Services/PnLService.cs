using MauiBackend.Models;
using MongoDB.Driver;

namespace MauiBackend.Services
{
    public class PnLService
    {
        private readonly IMongoCollection<PnLData> _pnlDataCollection;
        private readonly IServiceProvider _serviceProvider;
        private readonly MongoDbService _mongoDbService;
        private TradeDataService? _tradeDataService;
        public PnLService(IConfiguration config, MongoDbService mongoDbService, IServiceProvider serviceProvider)
        {

            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:Database"]);
            _pnlDataCollection = database.GetCollection<PnLData>("PnLData");
            _mongoDbService = mongoDbService;
            _serviceProvider = serviceProvider;
        }

        private TradeDataService TradeDataService => 
            _tradeDataService ??= _serviceProvider.GetRequiredService<TradeDataService>();

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

            List<PnLData> pnls = new List<PnLData>();
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


        public async Task<List<PnLData>> UpdatePnLBySeasonAsync(string id, string season)
        {
            var pnl = new List<PnLData>();

            var allTrades = await _tradeDataService.GetTrades(id);
            var trades = allTrades.Where(td => td.SeasonId == season).ToList();

            if (trades.Any())
            {
                var tradesByDate = trades.GroupBy(td => td.TradeDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalPnLPercent = g.Sum(t => t.PnLPercent)
                    })
                    .OrderBy(g => g.Date)
                    .ToList();

                pnl.Clear();
                foreach (var tradDate in tradesByDate)
                {
                    var tradePnL = new PnLData();
                    tradePnL.Date = tradDate.Date;
                    tradePnL.PnL = tradDate.TotalPnLPercent.Value;

                    pnl.Add(tradePnL);
                }
            }

            return pnl;
        }

    }
}
