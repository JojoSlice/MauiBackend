using MauiBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace MauiBackend.Controllers
{
    [Route("api/season")]
    [ApiController]

    public class SeasonController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;

        public SeasonController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        [HttpGet("get")]
        public async Task<Models.Season> GetSeasonAsync()
        {
            Console.WriteLine("Get season");
            var season = await _mongoDbService.GetCurrentSeason();
            return season;
        }
    }
}
