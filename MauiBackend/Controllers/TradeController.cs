using MauiBackend.Models;
using MauiBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace MauiBackend.Controllers
{
    [Route("api/trade")]
    [ApiController]
    public class TradeController : ControllerBase
    {
        private readonly TradeDataService _tradeDataService;
        public TradeController(TradeDataService tradeDataService)
        {
            _tradeDataService = tradeDataService;
        }

        [HttpPost("newTrade")]
        public async Task<IActionResult> CreateTrade([FromBody] Models.TradeData trade)
        {
            if (trade == null)
                return BadRequest("Invalid tradeData");
            else
                await _tradeDataService.AddTradeDataAsync(trade);

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
