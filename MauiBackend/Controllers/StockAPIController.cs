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
        private readonly string _apiKey = "dlXd5o50tlssBxnGjwBwRg==aM7Kx2NTPEY1tgwU";

        public StockAPIController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        [HttpGet("getassets")]
        public async Task<List<Models.Asset>> GetAssets()
        {
            Console.WriteLine("GetAsset startar");
            var assets = await _mongoDbService.GetAssets();
            if (assets.Count == 0)
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

                await _mongoDbService.AddManyAssets(assets);
            }
            Console.WriteLine("GetAsset slut");
            return assets;
        }

        [HttpGet("price")]
        public async Task<Models.Stock> GetAssetPrice([FromQuery] string ticker)
        {
            Console.WriteLine($"GetAssetPrice called, ticker: {ticker}");

            var url = $"https://api.api-ninjas.com/v1/stockprice?ticker={ticker}";

            using (_httpClient)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var stock = JsonSerializer.Deserialize<Models.Stock>(jsonResponse);
                        if (stock != null)
                        {
                            Console.WriteLine("data hämtad, GetAssetPrice slut");
                            return stock;
                        }
                        else
                        {
                            throw new Exception("stock data was null");
                        }
                    }
                    else
                    {
                        throw new Exception("faild api call");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new Models.Stock();
                }
            }
        }

        [HttpGet("stockprice")]
        public async Task<List<StockCandle>> GetStockData([FromQuery] string ticker, [FromQuery] string period)
        {
            Console.WriteLine($"GetStockData called, ticker: {ticker}, period: {period}");

            var url = $"https://api.api-ninjas.com/v1/stockpricehistorical?ticker={ticker}";

            using (_httpClient)
            {
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
                try
                {
                    var response = await _httpClient.GetAsync(url);

                    Console.WriteLine(response.ToString());

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var stockdata = JsonSerializer.Deserialize<List<StockCandle>>(jsonResponse);
                        if (stockdata != null)
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Error {ex.Message}");
                    return new List<StockCandle>();
                }
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
        public long Time { get; set; }
    }
}
