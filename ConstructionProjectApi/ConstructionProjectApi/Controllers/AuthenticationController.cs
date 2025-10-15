using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ConstructionProjectApi.Models.Auth;
using Asp.Versioning;

namespace ConstructionProjectApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/authentication")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IConfiguration configuration, ILogger<AuthenticationController> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate([FromBody] UserCredentials userCredentials)
        {
            var user = ValidateUser(userCredentials.Username, userCredentials.Password);

            if (user == null)
            {
                _logger.LogWarning("Authentication failed for user: {Username}", userCredentials.Username);
                return Unauthorized("Invalid username or password");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var secretKey = _configuration["Authentication:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogError("SecretKey is not configured in appsettings.json");
                return StatusCode(500, "Server configuration error.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration["Authentication:Issuer"],
                audience: _configuration["Authentication:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30), 
                signingCredentials: signingCredentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            _logger.LogInformation("User {Username} authenticated successfully.", user.Username);
            return Ok(new { user.Username, user.Role, Token = token });
        }

        
        private AliBabaUser? ValidateUser(string username, string password)
        {
            
            var validUsername = _configuration["AppSettings:AdminUsername"];
            var validPassword = _configuration["AppSettings:AdminPassword"];

            if (username == validUsername && password == validPassword)
            {
                return new AliBabaUser(username, "Admin", null);
            }
            return null;
        }
    }

    public class AliBabaUser
    {
        public string Username { get; set; }
        public string Role { get; set; }
        public string? Email { get; set; }

        public AliBabaUser(string username, string role, string? email)
        {
            Username = username;
            Role = role;
            Email = email;
        }
    }
}
