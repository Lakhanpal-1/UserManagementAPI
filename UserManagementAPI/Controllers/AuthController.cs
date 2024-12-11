using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementData.Dtos;
using UserManagementData.Entities;
using UserManagementData.Repository.IRepository;

namespace UserManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILoginRepository _loginRepository;
        private readonly IConfiguration _config;

        public AuthController(ILoginRepository loginRepository, IConfiguration config)
        {
            _loginRepository = loginRepository;
            _config = config;
        }



        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            // Validate login data
            if (login == null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            {
                return BadRequest("Email and password are required for login.");
            }

            try
            {
                var user = await _loginRepository.GetUserByEmailAndPassword(login.Email, login.Password);
                if (user == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                // Check if the user is deleted
                if (user.IsDeleted ?? false)
                {
                    return Unauthorized("User account is deleted.");
                }

                var tokenString = GenerateJwtToken(user);

                return Ok(new { Token = tokenString, UserId = user.Id, Role = user.Role });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Login failed: {ex.Message}");
            }
        }




        private string GenerateJwtToken(ApplicationUser user)
        {
            // Create the security key and signing credentials
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Define the claims that will be included in the token
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id), // User ID claim
        new Claim(ClaimTypes.Email, user.Email),       // Email claim
        new Claim(ClaimTypes.Role, user.Role),         // Role claim

        };

            // Create the JWT token with the claims
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],        // Issuer
                _config["Jwt:Audience"],      // Audience
                claims,                       // Claims
                expires: DateTime.Now.AddMinutes(30), // Expiry date
                signingCredentials: credentials // Signing credentials
            );

            // Return the generated token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
