﻿using MongoDB.Driver;
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
        private readonly IMongoCollection<PnLData> _pnlDataCollection;
        private readonly IMongoCollection<Season> _seasonsCollection;
        private readonly IMongoCollection<Asset> _assetsCollection;

        public MongoDbService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:Database"]);
            _usersCollection = database.GetCollection<User>("Users");
            _tradeDataCollection = database.GetCollection<TradeData>("TradeData");
            _pnlDataCollection = database.GetCollection<PnLData>("PnLData");
            _seasonsCollection = database.GetCollection<Season>("Seasons");
            _assetsCollection = database.GetCollection<Asset>("Assets");
        }

        public async Task<List<Asset>> GetAssets() =>
            await _assetsCollection.Find(asset => true).ToListAsync();

        public async Task AddAsset(Models.Asset asset) => 
            await _assetsCollection.InsertOneAsync(asset);

        public async Task AddManyAssets(List<Models.Asset> assets) =>
            await _assetsCollection.InsertManyAsync(assets);
        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            var existingUser = await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
            return existingUser != null;
        }

        public async Task<List<User>> GetUsersAsync() =>
            await _usersCollection.Find(user => true).ToListAsync();

        public async Task<User> GetUserByIdAsync(string id) =>
            await _usersCollection.Find(user => user.Id == id).FirstOrDefaultAsync();

        public async Task<User> GetUserByUsernameAsync(string username) =>
            await _usersCollection.Find(user => user.Username == username).FirstOrDefaultAsync();


        public async Task<bool> LoginAsync(LoginDto loginDto)
        {
            Console.WriteLine("Login Attempt");
            var user = await _usersCollection.Find(u => u.Username == loginDto.Username).FirstOrDefaultAsync();

            if (user == null)
            {
                Console.WriteLine("User not found");
                return false;
            }

            if(BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return true;
            }
            else
            {
                return false;
            }
        } 
        public async Task CreateUserAsync(User user)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            var pnl = new PnLData { UserId = user.Id };

            await _usersCollection.InsertOneAsync(user);
            await _pnlDataCollection.InsertOneAsync(pnl);
        }

        public async Task CreateSeason(Season season) => 
            await _seasonsCollection.InsertOneAsync(season);

        public async Task<Season> GetCurrentSeason()
        {
            var now = DateTime.UtcNow; 
            var filter = Builders<Season>.Filter.And(
                Builders<Season>.Filter.Lte(s => s.StartDate, now), 
                Builders<Season>.Filter.Gte(s => s.EndDate, now) 
            );

            return await _seasonsCollection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
