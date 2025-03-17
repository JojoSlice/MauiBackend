using MauiBackend;
using MauiBackend.Models;
using MauiBackend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

    [HttpGet("getuser")]
    public async Task<User> GetUser([FromQuery] string username)
    {
        Console.WriteLine("GetUser anropad");
        return await _mongoDbService.GetUserByUsernameAsync(username);
    }

    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser([FromBody] User user)
    {
        Console.WriteLine("Register called");
        Console.WriteLine(user.Username);
        Console.WriteLine(user.Name);
        
        if (await _mongoDbService.IsUsernameTakenAsync(user.Username))
        {
            return Conflict("Username already taken.");
        }
        await _mongoDbService.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
    }

    private const string SecretKey = "MegaHemligNyckel1337MegaHemligNyckel1337";
    private const int TokenExpiryMinute = 30;

    [HttpPost("login")]
    public async Task<ActionResult> LoginUser([FromBody]LoginDto loginDto)
    {
        if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return BadRequest("Invalid login request!");
        }

        bool loginSuccess = await _mongoDbService.LoginAsync(loginDto);

        if (loginSuccess)
        {
            var token = GenerateJwtTokens(loginDto.Username);
            return Ok(new { token });
        }
        
        return Unauthorized("Invalid username or password!");
    }
    
    private string GenerateJwtTokens(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "MauiBackend",
            audience: "MauiTrading",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(TokenExpiryMinute),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}