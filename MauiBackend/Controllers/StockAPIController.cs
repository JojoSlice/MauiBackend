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
        private readonly TradeDataService _tradeDataService;
        private readonly MongoDbService _mongoDbService;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiKey = "dlXd5o50tlssBxnGjwBwRg==aM7Kx2NTPEY1tgwU";

        public StockAPIController(MongoDbService mongoDbService, TradeDataService tradeDataService)
        {
            _mongoDbService = mongoDbService;
            _tradeDataService = tradeDataService;
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
        public async Task<IActionResult> GetAssetPrice([FromQuery] string ticker)
        {
            Console.WriteLine($"GetAssetPrice called, ticker: {ticker}");

            if (string.IsNullOrWhiteSpace(ticker))
            {
                return BadRequest("Ticker is required.");
            }

            var url = $"https://api.api-ninjas.com/v1/stockprice?ticker={ticker}";

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Response: {jsonResponse}");

                    var stock = JsonSerializer.Deserialize<Models.Stock>(jsonResponse);
                    if (stock != null)
                    {
                        Console.WriteLine("Data hämtad, GetAssetPrice slut");
                        await _tradeDataService.UpdateTradeAsync(stock.Price, stock.Ticker);
                        return Ok(stock);
                    }
                    else
                    {
                        return StatusCode(500, "Stock data was null.");
                    }
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode} - {errorResponse}");
                    return StatusCode((int)response.StatusCode, errorResponse);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("stockprice")]
        public async Task<IActionResult> GetStockData([FromQuery] string ticker, [FromQuery] string period)
        {
            Console.WriteLine($"GetStockData called, ticker: {ticker}, period: {period}");

            if (string.IsNullOrWhiteSpace(ticker))
            {
                return BadRequest("Ticker is required.");
            }

            var url = $"https://api.api-ninjas.com/v1/stockpricehistorical?ticker={ticker}";

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);

                var response = await _httpClient.GetAsync(url);
                Console.WriteLine($"API Response: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Data: {jsonResponse}");

                    var stockData = JsonSerializer.Deserialize<List<StockCandle>>(jsonResponse);
                    if (stockData != null)
                    {
                        Console.WriteLine("Data hämtad, GetStockData slut");
                        return Ok(stockData);
                    }
                    else
                    {
                        return StatusCode(500, "Stock data was null.");
                    }
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode} - {errorResponse}");
                    return StatusCode((int)response.StatusCode, errorResponse);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
