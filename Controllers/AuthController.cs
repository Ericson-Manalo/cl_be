using cl_be.Helpers;
using cl_be.Models;
using cl_be.Models.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace cl_be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly ClcredsDbContext _context;
        private JwtSettings _jwtSettings;

        public AuthController(ClcredsDbContext context, JwtSettings jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginCredentials loginCredentials)
        {
            var user = await _context.UserLogins
                .FirstOrDefaultAsync(u => u.Email == loginCredentials.Email);

            if (user == null)
                return Unauthorized("Email is not registered");

            bool isValid = PasswordHelper.VerifyPassword(
                loginCredentials.Password,
                user.PasswordHash,
                user.PasswordSalt
            );

            if (!isValid)
                return Unauthorized("Password invalid");

            // Check user's role
            string role = RoleToString(user.Role);

            // Da definire il token:
            var token = GenerateJwt(loginCredentials, role);

            return Ok(new { Message = "Login successful", token});
        }

        [HttpPost("PasswordReset")]
        public async Task<IActionResult> PasswordReset(ResetPassword resetPassword)
        {
            // verifica di nuovo se l'email esiste
            var user = await _context.UserLogins
                .FirstOrDefaultAsync(u => u.Email == resetPassword.Email);

            if (user == null)
                return Unauthorized("Email is not registered");

            var newSalt = PasswordHelper.GenerateSalt();
            var newHash = PasswordHelper.GenerateHash(resetPassword.NewPassword, newSalt);

            user.PasswordSalt = newSalt;
            user.PasswordHash = newHash;
            user.AsUpdated = true;

            await _context.SaveChangesAsync();

            return Ok("Password has been changed successfully");
        }

        private string GenerateJwt(LoginCredentials loginCredentials, string role)
        {
            var secretKey = _jwtSettings.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity([
                    new Claim(ClaimTypes.Email, loginCredentials.Email),
                    new Claim(ClaimTypes.Role, role)
                ]),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        private string RoleToString(byte role)
        {
            switch(role)
            {
                case 2:
                    return "User";
                case 3:
                    return "Admin";
                default:
                    break;
            }

            return "Guest";
        }
    }
}
