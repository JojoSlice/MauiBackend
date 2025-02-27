using Microsoft.AspNetCore.Mvc;
using MauiBackend.Services;
using MauiBackend.Models;
namespace MauiBackend.Controllers
{

    [ApiController]
    [Route("api/pnl")]
    public class PnLController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;
        private readonly PnLService _pnlService;
        public PnLController(MongoDbService mongoDbService, PnLService pnLService)
        {
            _mongoDbService = mongoDbService;
            _pnlService = pnLService;
        }

        [HttpPost("getpnl")]
        public async Task<List<PnLData>> GetPnLDataAsync(string username)
        {
            Console.WriteLine("Getting pnl");

            var user = await _mongoDbService.GetUserByUsernameAsync(username);
            Console.WriteLine($"Found {user.Username}");

            var pnl = await _pnlService.UpdatePnLAsync(user.Id);

            return pnl;
        }
    }
}
