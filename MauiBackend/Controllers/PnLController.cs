using Microsoft.AspNetCore.Mvc;
using MauiBackend.Services;
using MauiBackend.Models;
namespace MauiBackend.Controllers
{

    [Route("api/pnl")]
    [ApiController]
    public class PnLController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;
        private readonly PnLService _pnlService;
        public PnLController(MongoDbService mongoDbService, PnLService pnLService)
        {
            _mongoDbService = mongoDbService;
            _pnlService = pnLService;
        }

        [HttpGet("get")]
        public async Task<List<PnLData>> GetPnLDataAsync([FromQuery]string username)
        {
            Console.WriteLine("Getting pnl");

            var user = await _mongoDbService.GetUserByUsernameAsync(username);
            Console.WriteLine($"Found {user.Username}");

            var pnl = await _pnlService.GetPnLAsync(user.Id);

            return pnl;
        }
    }
}
