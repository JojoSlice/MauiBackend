using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MauiBackend.Controllers
{
    [ApiController]
    [Route("api/stocks")]
    public class StockAPIController : Controller
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiKey = "curg10hr01qgoblekt90curg10hr01qgoblekt9g";

        [HttpGet("stockprice")]
        public async Task<List<StockCandle>> GetStockData(string ticker, string period)
        {
            Console.WriteLine("GetStockData called");

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.api-ninjas.com/v1/stockpricehistorical?ticker={ticker}&period={period}");
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
