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
        private readonly AdventureWorksLt2019Context _adventureContext;
        private JwtSettings _jwtSettings;

        public AuthController(ClcredsDbContext context, JwtSettings jwtSettings, AdventureWorksLt2019Context adventureContext)
        {
            _context = context;
            _jwtSettings = jwtSettings;
            _adventureContext = adventureContext;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginCredentials loginCredentials)
        {
            var user = await _context.UserLogins
                .FirstOrDefaultAsync(u => u.Email == loginCredentials.Email);

            if (user == null)
                return Unauthorized(new { message = "Email is not registered" });

            bool isValid = PasswordHelper.VerifyPassword(
                loginCredentials.Password,
                user.PasswordHash,
                user.PasswordSalt
            );

            if (!isValid && user.AsUpdated == false)
                return StatusCode(409, new { requiresPasswordUpdate = true });

            if (!isValid)
                return Unauthorized(new { message = "Password invalid" });

            // Prendo Id del customer
            int id = user.CustomerId;

            // Check user's role
            string role = user.Role == 2 ? "Admin" : "User";

            // JWT
            var token = GenerateJwt(loginCredentials, id, role);

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

            return Ok(new { message = "Password has been changed successfully" });
        }

        // Creazione Logica della Registrazione utente/customer
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterCredentials registerCredentials)
        {
            // Validazione del data model
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Controllo se l'email è già nel database
            var existingCustomer = await _context.UserLogins
                .FirstOrDefaultAsync(c => c.Email == registerCredentials.Email);

            if (existingCustomer != null)
                return Conflict(new { message = "Email is already registered" });

            // Salvo il nuovo Customer nel db originale
            var customer = new Customer
            {
                FirstName = registerCredentials.Name,
                LastName = registerCredentials.Surname,
                MiddleName = registerCredentials.Middlename,
                EmailAddress = registerCredentials.Email,
                Phone = registerCredentials.Phone,

                // I campi hash e salt non possono essere NULL quindi mettiamo dei dummy
                //PasswordHash = "",
                //PasswordSalt = ""
            };

            _adventureContext.Customers.Add(customer);
            await _adventureContext.SaveChangesAsync();

            // Salvo i credenziali del nuovo Customer nell'altro db
            var customerId = customer.CustomerId;
            var salt = PasswordHelper.GenerateSalt();
            var hash = PasswordHelper.GenerateHash(registerCredentials.Password, salt);

            var userLogin = new UserLogin
            {
                CustomerId = customerId,
                Email = registerCredentials.Email,
                PasswordSalt = salt,
                PasswordHash = hash,
                AsUpdated = false,
                Role = 1
            };

            _context.UserLogins.Add(userLogin);
            await _context.SaveChangesAsync();

            // Ritorna 201 created (senza salvataggio del password non-hashato)
            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Customer registered successfully.",
                customer = new
                {
                    customer.CustomerId,
                    customer.FirstName,
                    customer.LastName,
                    customer.MiddleName,
                    customer.EmailAddress,
                    customer.Phone
                }
            });
        }

        private string GenerateJwt(LoginCredentials loginCredentials, int id, string role)
        {
            var secretKey = _jwtSettings.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity([
                    new Claim("CustomerId", id.ToString()),
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
    }

}
