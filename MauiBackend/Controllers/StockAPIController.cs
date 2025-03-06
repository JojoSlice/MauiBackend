using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;
using MauiBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MauiBackend.Controllers
{
    [ApiController]
    [Route("api/stocks")]
    public class StockAPIController : Controller
    {
        private readonly MongoDbService _mongoDbService;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiKey = "curg10hr01qgoblekt90curg10hr01qgoblekt9g";

        public StockAPIController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        [HttpGet("getassets")]
        public async Task<List<Models.Asset>> GetAssets()
        {
            Console.WriteLine("GetAsset startar");
            var assets = await _mongoDbService.GetAssets();
            if (assets == null)
            {
                assets = new List<Models.Asset>
                {
                    new Models.Asset { Name = "Tesla", Ticker = "TSLA" },
                    new Models.Asset { Name = "Apple", Ticker = "AAPL" },
                    new Models.Asset { Name = "Microsoft", Ticker = "MSFT" },
                    new Models.Asset { Name = "NIO", Ticker = "NIO" },
                    new Models.Asset { Name = "AMC", Ticker = "AMC" },
                    new Models.Asset { Name = "AMC Pref", Ticker = "APE" },
                    new Models.Asset { Name = "NVIDIA", Ticker = "NVDA" },
                    new Models.Asset { Name = "GameStop", Ticker = "GME" },
                    new Models.Asset { Name = "Meta", Ticker = "META" },
                    new Models.Asset { Name = "Amazon", Ticker = "AMZN" }
                };
                foreach(var asset in assets)
                {
                    await _mongoDbService.AddAsset(asset);
                }
            }
            Console.WriteLine("GetAsset slut");
            return assets;
        }

        [HttpGet("stockprice")]
        public async Task<List<StockCandle>> GetStockData(Models.Asset stock)
        {
            Console.WriteLine("GetStockData called");

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.api-ninjas.com/v1/stockpricehistorical?ticker={stock.Ticker}&period={stock.Period}");
            request.Headers.Add("X-Api-Key", _apiKey);
            try
            {
                var response = await _httpClient.SendAsync(request);

                if(response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var stockdata = JsonSerializer.Deserialize<List<StockCandle>>(jsonResponse);
                    if(stockdata != null)
                    {
                        Console.WriteLine("data hämtad, GetStockData slut");
                        return stockdata;
                    }
                    else
                    {
                        throw new Exception("stockdata not found");
                    }
                }
                else
                {
                    throw new Exception("failed to get stockdata from api ninjas");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}");
                return null;
            }
        }
    }

    public class StockCandle
    {
        [JsonPropertyName("open")]
        public double Open { get; set; }
        [JsonPropertyName("low")]
        public double Low { get; set; }
        [JsonPropertyName("high")]
        public double High { get; set; }
        [JsonPropertyName("close")]
        public double Close { get; set; }
        [JsonPropertyName("volume")]
        public double Volume { get; set; }
        [JsonPropertyName("time")]
        public string Time { get; set; }
    }
}
