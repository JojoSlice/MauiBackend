using MauiBackend.Models;
using MauiBackend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly MongoDbService _mongoDbService;
    public UserController(MongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    [HttpGet]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        return await _mongoDbService.GetUsersAsync();
    }

    [HttpPost]
    public async Task<ActionResult> RegisterUser(User user)
    {
        if (await _mongoDbService.IsUsernameTakenAsync(user.Username))
        {
            return Conflict("Username already taken.");
        }
        await _mongoDbService.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
    }
    [HttpPost("login")]
    public async Task<ActionResult> LoginUser([FromBody]LoginDto loginDto)
    {
        if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return BadRequest("Invalid login request!");
        }

        bool loginSuccess = await _mongoDbService.LoginAsync(loginDto);

        if (!loginSuccess)
        {
            return Unauthorized("Invalid username or password!");
        }

        return Ok(new { Message = "Login successfull!" });
    }
}