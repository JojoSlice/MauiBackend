using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using MauiBackend.Models;
using Microsoft.AspNetCore.Identity;

namespace MauiBackend.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<TradeData> _tradeDataCollection;

        public MongoDbService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:Database"]);
            _usersCollection = database.GetCollection<User>("Users");
            _tradeDataCollection = database.GetCollection<TradeData>("TradeData");
        }


        //Allt som rör User--------------------------------------------------------------------------------------
        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            var existingUser = await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
            return existingUser != null; // true om användaren finns
        }

        public async Task<List<User>> GetUsersAsync() =>
            await _usersCollection.Find(user => true).ToListAsync();

        public async Task<User> GetUserByIdAsync(string id) =>
            await _usersCollection.Find(user => user.Id == id).FirstOrDefaultAsync();

        public async Task<bool> LoginAsync(LoginDto loginDto)
        {
            var user = await _usersCollection.Find(u => u.Username == loginDto.Username).FirstOrDefaultAsync();

            Console.WriteLine($"Användare loggar in: {loginDto.Username} Pass: {loginDto.Password}");
            Console.WriteLine($"Mongo hittade {user.Username}");
            if (user == null)
            {
                Console.WriteLine("Användare hittades inte!");
                return false;
            }

            if(BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                Console.WriteLine("Lösenord matchade");
                return true;
            }
            else
            {
                Console.WriteLine("Lösenordet matchade inte, kolla över rehash");
                return false;
            }
        } 
        public async Task CreateUserAsync(User user)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            await _usersCollection.InsertOneAsync(user);
        }



        //Allt som rör TradeData--------------------------------------------------------------------------------------
        public async Task AddTradeDataAsync(TradeData tradeData)
        {
            await _tradeDataCollection.InsertOneAsync(tradeData);
        }

        public async Task<List<TradeData>> GetAllTradesByUserIdAsync(string userId)
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

        public async Task CloseTradeAsync(string tradeId, double exitPrice)
        {
            var filter = Builders<TradeData>.Filter.Eq(td => td.Id, tradeId);
            var trade = await _tradeDataCollection.Find(filter).FirstOrDefaultAsync();

            if (trade != null && trade.IsOpen)
            {
                double pnl = 0;
                if (trade.IsLong)
                {
                    pnl = (exitPrice - trade.Price) * trade.Quantity;
                }
                else
                {
                    pnl = (trade.Price - exitPrice) * trade.Quantity;
                }

                var update = Builders<TradeData>.Update
                    .Set(td => td.IsOpen, false)
                    .Set(td => td.PnL, pnl)
                    .Set(td => td.ClosingPrice, exitPrice);

                await _tradeDataCollection.UpdateOneAsync(filter, update);
            }

        }

        //Lös senare!
        //Kan vara bra att se till att varken takeprofit eller stoploss hamnar för nära currentPrice
        public async Task UpdateTakeProfitAsync(string tradeId, double takeProfit)
        {
            var filter = Builders<TradeData>.Filter.Eq(td => td.Id, tradeId);
            var trade = await _tradeDataCollection.Find(filter).FirstOrDefaultAsync();

            if (trade != null)
                trade.TakeProfit = takeProfit;
        }
        public async Task UpdateStopLossAsync(string tradeId, double stopLoss)
        {
            var filter = Builders<TradeData>.Filter.Eq(td => td.Id, tradeId);
            var trade = await _tradeDataCollection.Find(filter).FirstOrDefaultAsync();

            if (trade != null)
                trade.StopLoss = stopLoss;
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
                    pnl = (currentPrice - trade.Price) * trade.Quantity;
                    takeProfitReached = currentPrice >= trade.TakeProfit;
                    stopLossReached = currentPrice <= trade.StopLoss;
                }
                else
                {
                    pnl = (trade.Price - currentPrice) * trade.Quantity;
                    takeProfitReached = currentPrice <= trade.TakeProfit;
                    stopLossReached = currentPrice >= trade.StopLoss;
                }

                if (takeProfitReached || stopLossReached)
                {
                    if (trade.IsLong)
                    {
                        pnl = (currentPrice - trade.Price) * trade.Quantity;
                    }
                    else
                    {
                        pnl = (trade.Price - currentPrice) * trade.Quantity;
                    }

                    var update = Builders<TradeData>.Update
                    .Set(td => td.IsOpen, false)
                    .Set(td => td.PnL, trade.PnL);

                    await _tradeDataCollection.UpdateOneAsync(
                        Builders<TradeData>.Filter.Eq(td => td.Id, trade.Id),
                        update);
                }
                else
                {
                    var update = Builders<TradeData>.Update.Set(td => td.PnL, pnl);
                    await _tradeDataCollection.UpdateOneAsync(
                        Builders<TradeData>.Filter.Eq(td => td.Id, trade.Id), update);
                }
            }
        }
    }
}
