using MauiBackend.Models;
using MongoDB.Driver;

namespace MauiBackend.Services
{
    public class TradeDataService
    {
        private readonly IMongoCollection<TradeData> _tradeDataCollection;
        public TradeDataService(IConfiguration config, MongoDbService mongoDbService, PnLService pnLService)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:Database"]);
            _tradeDataCollection = database.GetCollection<TradeData>("TradeData");
        }
        public async Task AddTradeDataAsync(TradeData tradeData)
        {
            await _tradeDataCollection.InsertOneAsync(tradeData);
        }

        public async Task<TradeData> GetTradeById(string id)
        {
            return await _tradeDataCollection.Find(trade => trade.Id == id).FirstOrDefaultAsync();
        }


        public async Task<List<TradeData>> GetClosedTradesByUserIdAsync(string userId)
        {
            var filter = Builders<TradeData>.Filter.Eq(td => td.UserId, userId);
            var trades = await _tradeDataCollection.Find(filter).ToListAsync();

            return trades.Where(t => t.IsOpen == false).ToList();
        }

        public async Task<List<TradeData>> GetTradesBySeason(string userId, string seasonId)
        {
            var filter = Builders<TradeData>.Filter.And(
                Builders<TradeData>.Filter.Eq(td => td.UserId, userId),
                Builders<TradeData>.Filter.Eq(td => td.SeasonId, seasonId)
                );

            return await _tradeDataCollection.Find(filter).ToListAsync();
        }

        public async Task<List<TradeData>> GetOpenOrClosedTradesByUserIdAsync(string userId, bool isOpen)
        {
            var filter = Builders<TradeData>.Filter.And(
                Builders<TradeData>.Filter.Eq(td => td.UserId, userId),
                Builders<TradeData>.Filter.Eq(td => td.IsOpen, isOpen));

            return await _tradeDataCollection.Find(filter).ToListAsync();
        }

        public async Task CloseTradeAsync(string tradeId, double exitPrice)
        {
            var filter = Builders<TradeData>.Filter.Eq(td => td.Id, tradeId);
            var trade = await _tradeDataCollection.Find(filter).FirstOrDefaultAsync();

            if (trade != null && trade.IsOpen)
            {
                double pnl = 0;
                if (trade.IsLong)
                {
                    pnl = (exitPrice - trade.Price) * trade.PointsUsed;
                }
                else
                {
                    pnl = (trade.Price - exitPrice) * trade.PointsUsed;
                }

                var update = Builders<TradeData>.Update
                    .Set(td => td.IsOpen, false)
                    .Set(td => td.PnLPercent, pnl)
                    .Set(td => td.ClosingPrice, exitPrice);

                await _tradeDataCollection.UpdateOneAsync(filter, update);
            }

        }
        //Lös/Kontrollera senare!
        //Kan vara bra att se till att varken takeprofit eller stoploss hamnar för nära currentPrice
        public async Task UpdateTakeProfitAsync(string tradeId, double takeProfit, double currentPrice)
        {
            var filter = Builders<TradeData>.Filter.Eq(td => td.Id, tradeId);
            var trade = await _tradeDataCollection.Find(filter).FirstOrDefaultAsync();


            if (trade != null)
            {
                if (takeProfit <= currentPrice)
                    return;
                else
                    trade.TakeProfit = takeProfit;
            } 
        }
        public async Task UpdateStopLossAsync(string tradeId, double stopLoss, double currentPrice)
        {
            var filter = Builders<TradeData>.Filter.Eq(td => td.Id, tradeId);
            var trade = await _tradeDataCollection.Find(filter).FirstOrDefaultAsync();

            if (trade != null)
            {
                if (stopLoss >= currentPrice)
                    return;
                else
                    trade.StopLoss = stopLoss;
            }
        }

        public async Task UpdateTradeAsync(string userId, double currentPrice)
        {
            var filter = Builders<TradeData>.Filter.And(
                Builders<TradeData>.Filter.Eq(td => td.UserId, userId),
                Builders<TradeData>.Filter.Eq(td => td.IsOpen, true));

            var trades = await _tradeDataCollection.Find(filter).ToListAsync();

            foreach(var trade in trades)
            {
                double pnl = 0;
                bool takeProfitReached = false;
                bool stopLossReached = false;

                if (trade.IsLong)
                {
                    pnl = (currentPrice - trade.Price) * trade.PointsUsed;
                    takeProfitReached = currentPrice >= trade.TakeProfit;
                    stopLossReached = currentPrice <= trade.StopLoss;
                }
                else
                {
                    pnl = (trade.Price - currentPrice) * trade.PointsUsed;
                    takeProfitReached = currentPrice <= trade.TakeProfit;
                    stopLossReached = currentPrice >= trade.StopLoss;
                }

                if (takeProfitReached || stopLossReached)
                {
                    if (trade.IsLong)
                    {
                        pnl = (currentPrice - trade.Price) * trade.PointsUsed;
                    }
                    else
                    {
                        pnl = (trade.Price - currentPrice) * trade.PointsUsed;
                    }

                    var update = Builders<TradeData>.Update
                    .Set(td => td.IsOpen, false)
                    .Set(td => td.PnLPercent, trade.PnLPercent);

                    await _tradeDataCollection.UpdateOneAsync(
                        Builders<TradeData>.Filter.Eq(td => td.Id, trade.Id),
                        update);
                }
                else
                {
                    var update = Builders<TradeData>.Update.Set(td => td.PnLPercent, pnl);
                    await _tradeDataCollection.UpdateOneAsync(
                        Builders<TradeData>.Filter.Eq(td => td.Id, trade.Id), update);
                }
            }
        }

    }
}
