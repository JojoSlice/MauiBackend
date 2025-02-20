using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MauiBackend.Controllers
{
    [ApiController]
    [Route("api/market")]
    public class StockAPIController : Controller
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiKey = "curg10hr01qgoblekt90curg10hr01qgoblekt9g";
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(10);

        public StockAPIController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet("status")]
        public async Task<MarketStatus> GetMarketStatus()
        {
            if (_cache.TryGetValue("MarketStatus", out MarketStatus cachedStatus))
            {
                return cachedStatus;
            }

            var url = $"https://finnhub.io/api/v1/stock/market-status?exchange=US&token={_apiKey}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var marketStatus = JsonSerializer.Deserialize<MarketStatus>(jsonResponse);
                    if (marketStatus != null)
                    {
                        _cache.Set("MarketStatus", marketStatus, _cacheDuration);
                        return marketStatus;
                    }
                    else
                        throw new Exception("MarketStatus was not found");
                }
                else
                {
                    throw new Exception("Failed to fetch data from Finnhub API");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
        public class MarketStatus
        {
            [JsonPropertyName("exchange")]
            public string Exchange { get; set; }
            [JsonPropertyName("holiday")]
            public string? Holiday { get; set; }
            [JsonPropertyName("isOpen")]
            public bool IsOpen { get; set; }
            [JsonPropertyName("session")]
            public string? CurrentSession { get; set; }
        }
    }
}
