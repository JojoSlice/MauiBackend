using MauiBackend.Models;
using MauiBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MauiBackend.Controllers
{
    [Route("api/trade")]
    [ApiController]
    public class TradeController : ControllerBase
    {
        private readonly TradeDataService _tradeDataService;
        private readonly MongoDbService _mongoDbService;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiKey = "dlXd5o50tlssBxnGjwBwRg==aM7Kx2NTPEY1tgwU";

        public TradeController(TradeDataService tradeDataService, MongoDbService mongoDbService)
        {
            _tradeDataService = tradeDataService;
            _mongoDbService = mongoDbService;
        }
        [HttpPost("closetrade")]
        public async Task<IActionResult> CloseTrade([FromBody] Models.TradeData trade)
        {
            Console.WriteLine("CloseTrade start");
            if(trade == null)
            {
                return BadRequest("Invalid TradeData");
            }
            
            await _tradeDataService.CloseTradeAsync(trade);
            Console.WriteLine("End");
            return NoContent();
        }

        [HttpGet("tradeseasonalhistory")]
        public async Task<List<TradeData>> GetSeasonalTradeHistory([FromQuery] string userId)
        {
            Console.WriteLine("TradeHistory start");
            var season = await _mongoDbService.GetCurrentSeason();
            Console.WriteLine("End");
            return await _tradeDataService.GetTradesBySeason(userId, season.Id);
        }

        [HttpPost("newtrade")]
        public async Task<IActionResult> CreateTrade([FromBody] Models.TradeData trade)
        {
            Console.WriteLine("NewTrade start");
            if (trade == null)
                return BadRequest("Invalid tradeData");
            else
            {
                if (string.IsNullOrWhiteSpace(trade.Id))
                {
                    trade.Id = null;
                }
                await _tradeDataService.AddTradeDataAsync(trade);
                var user = await _mongoDbService.GetUserByIdAsync(trade.UserId);
                user.Points = user.Points - trade.PointsUsed;
                await _mongoDbService.UpdateUserPointsAsync(user);
            }

            Console.WriteLine("End");
            return CreatedAtAction(nameof(GetTrade), new { id = trade.Id }, trade); 
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<TradeData>> GetTrade(string id)
        {
            var trade = await _tradeDataService.GetTradeById(id);

            if (trade == null)
            {
                return NotFound();
            }
            return trade;
        }


    }
}
