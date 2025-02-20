using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MauiBackend.Controllers
{
    [ApiController]
    [Route("api/stocks")]
    public class StockAPIController : Controller
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiKey = "curg10hr01qgoblekt90curg10hr01qgoblekt9g";


        [HttpGet("prices")]
        public async Task<MarketStatus> GetStockPrices()
        {
            var url = $"https://finnhub.io/api/v1/stock/market-status?exchange=US&token={_apiKey}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var marketStatus = JsonSerializer.Deserialize<MarketStatus>(jsonResponse);
                    return marketStatus;
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
            public string Market { get; set; }
            public bool IsOpen { get; set; }
            public string CurrentSession { get; set; }
        }
    }
}
