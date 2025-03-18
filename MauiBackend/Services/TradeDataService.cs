using MauiBackend.Models;
using MongoDB.Driver;

namespace MauiBackend.Services
{
    public class TradeDataService
    {
        private readonly MongoDbService _mongoDbService;
        private readonly IMongoCollection<TradeData> _tradeDataCollection;
        private readonly PnLService _pnLService;
        public TradeDataService(MongoDbService mongoDbService, PnLService pnLService)
        {
            _mongoDbService = mongoDbService;
            _tradeDataCollection = _mongoDbService.GetTradeDataCollection();
            _pnLService = pnLService;
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

        public async Task<List<TradeData>> GetTrades(string userId)
        {
            var filter = Builders<TradeData>.Filter.Eq(td => td.UserId, userId);

            return await _tradeDataCollection.Find(filter).ToListAsync();
        }

        public async Task<List<TradeData>> GetOpenOrClosedTradesByUserIdAsync(string userId, bool isOpen)
        {
            var filter = Builders<TradeData>.Filter.And(
                Builders<TradeData>.Filter.Eq(td => td.UserId, userId),
                Builders<TradeData>.Filter.Eq(td => td.IsOpen, isOpen));

            return await _tradeDataCollection.Find(filter).ToListAsync();
        }

        public async Task CloseTradeAsync(TradeData closeTrade)
        {
            var filter = Builders<TradeData>.Filter.Eq(td => td.Id, closeTrade.Id);
            var trade = await _tradeDataCollection.Find(filter).FirstOrDefaultAsync();

            if (trade != null && trade.IsOpen)
            {
                double pnlPercentage = 0;
                double pnl = 0;

                if (closeTrade.ClosingPrice != null)
                {
                    pnlPercentage = ((closeTrade.ClosingPrice.Value - trade.Price) / trade.Price) * 100;

                    if (!trade.IsLong)
                    {
                        pnlPercentage *= -1;
                    }

                    pnl = (pnlPercentage / 100) * trade.PointsUsed;
                }

                var updateTrade = Builders<TradeData>.Update
                    .Set(td => td.IsOpen, false)
                    .Set(td => td.PnLPercent, pnl)
                    .Set(td => td.ClosingPrice, closeTrade.ClosingPrice);

                var user = await _mongoDbService.GetUserByIdAsync(trade.UserId);

                user.Points += (int)Math.Round(trade.PointsUsed * (1 + pnlPercentage / 100));

                await _mongoDbService.UpdateUserPointsAsync(user.Id, user.Points);

                await _tradeDataCollection.UpdateOneAsync(filter, updateTrade);

                await _pnLService.UpdatePnL(trade);
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

        public async Task UpdateTradeAsync(double currentPrice, string ticker)
        {
            var filter = Builders<TradeData>.Filter.Eq(td => td.Ticker, ticker);
            var trades = await _tradeDataCollection.Find(filter).ToListAsync();

            if (trades.Count > 0)
            {

                foreach (var trade in trades)
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
                            pnl = (trade.StopLoss.Value - trade.Price) * trade.PointsUsed;
                        }
                        else
                        {
                            pnl = (trade.Price - trade.StopLoss.Value) * trade.PointsUsed;
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
}
