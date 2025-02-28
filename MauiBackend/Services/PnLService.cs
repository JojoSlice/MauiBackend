using MauiBackend.Models;
using MongoDB.Driver;

namespace MauiBackend.Services
{
    public class PnLService
    {
        private readonly IMongoCollection<PnLData> _pnlDataCollection;
        private readonly TradeDataService _tradeDataService;
        public PnLService(IConfiguration config, MongoDbService mongoDbService, TradeDataService tradeDataService)
        {

            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:Database"]);
            _pnlDataCollection = database.GetCollection<PnLData>("PnLData");
            _tradeDataService = tradeDataService;
        }
        public async Task<PnLData> GetPnLAsync(string id) =>
            await _pnlDataCollection.Find(pnl => pnl.UserId == id).FirstOrDefaultAsync();
        public async Task<List<PnLData>> UpdatePnLAsync(string id)
        {
            var pnl = new List<PnLData>();
            var trades = await _tradeDataService.GetClosedTradesByUserIdAsync(id);


            if (trades.Any())
            {
                var tradesByDate = trades.GroupBy(td => td.TradeDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalPnL = g.Sum(t => t.PnL)
                    })
                    .OrderBy(g => g.Date)
                    .ToList();

                pnl.Clear();
                foreach (var tradDate in tradesByDate)
                {
                    var tradePnL = new PnLData();
                    tradePnL.Date = tradDate.Date;
                    tradePnL.PnL = tradDate.TotalPnL.Value;

                    pnl.Add(tradePnL);
                }
            }

            return pnl;
        }

    }
}
