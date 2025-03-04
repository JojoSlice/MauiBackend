using MauiBackend.Models;
using MongoDB.Driver;

namespace MauiBackend.Services
{
    public class PnLService
    {
        private readonly IMongoCollection<PnLData> _pnlDataCollection;
        private readonly TradeDataService _tradeDataService;
        private readonly MongoDbService _mongoDbService;
        public PnLService(IConfiguration config, MongoDbService mongoDbService, TradeDataService tradeDataService)
        {

            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:Database"]);
            _pnlDataCollection = database.GetCollection<PnLData>("PnLData");
            _tradeDataService = tradeDataService;
            _mongoDbService = mongoDbService;
        }

        public async Task<List<PnLData>> GetPnLAsync(string userId)
        {
            var season = await _mongoDbService.GetCurrentSeason();

            var filter = Builders<PnLData>.Filter.And(
                Builders<PnLData>.Filter.Eq(pnl => pnl.UserId, userId),
                Builders<PnLData>.Filter.Gte(pnl => pnl.Date, season.StartDate),
                Builders<PnLData>.Filter.Lte(pnl => pnl.Date, season.EndDate));

            return await _pnlDataCollection.Find(filter).ToListAsync();
        }

        public async Task<PnLData> GetTodaysPnL(string userId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var filter = Builders<PnLData>.Filter.And(
                Builders<PnLData>.Filter.Eq(pnl => pnl.UserId, userId),
                Builders<PnLData>.Filter.Gte(pnl => pnl.Date, today),
                Builders<PnLData>.Filter.Lt(pnl => pnl.Date, tomorrow));

            var pnl = await _pnlDataCollection.Find(filter).FirstOrDefaultAsync();

            if (pnl == null)
            {
                pnl = new PnLData { UserId = userId, Date = DateTime.UtcNow };
                await _pnlDataCollection.InsertOneAsync(pnl);
            }

            return pnl;
        }

        //public async Task<List<PnLData>> UpdatePnLBySeasonAsync(string id, string season)
        //{
        //    var pnl = new List<PnLData>();
            
        //    var trades = await _tradeDataService.GetTradesBySeason(id, season);

        //    if (trades.Any())
        //    {
        //        var tradesByDate = trades.GroupBy(td => td.TradeDate.Date)
        //            .Select(g => new
        //            {
        //                Date = g.Key,
        //                TotalPnLPercent = g.Sum(t => t.PnLPercent)
        //            })
        //            .OrderBy(g => g.Date)
        //            .ToList();

        //        pnl.Clear();
        //        foreach (var tradDate in tradesByDate)
        //        {
        //            var tradePnL = new PnLData();
        //            tradePnL.Date = tradDate.Date;
        //            tradePnL.PnLPercent = tradDate.TotalPnLPercent.Value;

        //            pnl.Add(tradePnL);
        //        }
        //    }

        //    return pnl;
        //}

    }
}
