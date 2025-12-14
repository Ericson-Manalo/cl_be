using cl_be.Helpers;
using cl_be.Models;
using cl_be.Models.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            bool isValid = PasswordHelper.VerifyPassword(
                loginCredentials.Password,
                user.PasswordHash,
                user.PasswordSalt
            );

            if (!isValid && user.AsUpdated == false)
                return StatusCode(409, new { requiresPasswordUpdate = true });

            if (!isValid)
                return Unauthorized(new { message = "Invalid email or password" });

            // Prendo Id del customer
            int id = user.CustomerId;

            // Check user's role
            string role = user.Role == 2 ? "Admin" : "User";

            // JWT
            var token = GenerateJwt(loginCredentials, id, role);

            var refreshToken = GenerateRefreshToken();

            // Save refresh token in DB (CustomerId FK)
            await SaveRefreshTokenToDb(id, refreshToken);

            SetRefreshTokenCookie(refreshToken);

            return Ok(new { message = "Login successful", token});
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
                FirstName = registerCredentials.FirstName,
                LastName = registerCredentials.LastName,
                MiddleName = registerCredentials.MiddleName,
                EmailAddress = registerCredentials.Email,
                Phone = registerCredentials.Phone,

                // I campi hash e salt non possono essere NULL quindi mettiamo dei dummy
                //PasswordHash = "",
                //PasswordSalt = ""
            };

            _adventureContext.Customers.Add(customer);
            await _adventureContext.SaveChangesAsync();

            // Salvo i credenziali del nuovo Customer nell'altro db
            var salt = PasswordHelper.GenerateSalt();
            var hash = PasswordHelper.GenerateHash(registerCredentials.Password, salt);

            var userLogin = new UserLogin
            {
                CustomerId = customer.CustomerId,
                Email = registerCredentials.Email,
                PasswordSalt = salt,
                PasswordHash = hash,
                AsUpdated = false,
                Role = 1
            };

            _context.UserLogins.Add(userLogin);
            await _context.SaveChangesAsync();

            // Ritorna 201 created (senza salvataggio del password non-hashato)
            // Questo serve solo per far vedere lo status dell'operazione della registrazione
            //return StatusCode(StatusCodes.Status201Created, new
            //{
            //    message = "Customer registered successfully.",
            //    customer = new
            //    {
            //        customer.CustomerId,
            //        customer.FirstName,
            //        customer.LastName,
            //        customer.MiddleName,
            //        customer.EmailAddress,
            //        customer.Phone
            //    }
            //});

            // logica dell'accesso automatico
            string role = "User";
            var loginCredentials = new LoginCredentials
            {
                Email = registerCredentials.Email,
                Password = registerCredentials.Password,
            };

            string token = GenerateJwt(loginCredentials, customer.CustomerId, role);

            return Ok(new
            {
                message = "Registration successful. Logged in automatically.", token
            });
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshTokenValue = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshTokenValue))
            {
                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue);

                if (storedToken != null)
                {
                    storedToken.IsRevoked = true;
                    storedToken.ModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                Response.Cookies.Delete("refreshToken");
            }

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh()
        {
            // Some variables for printing the current time in the console:
            var utcNow = DateTime.UtcNow;
            var tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var timeUtcPlus1 = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($">> REFRESH TOKEN REQUEST RECEIVED at {timeUtcPlus1:yyyy-MM-dd HH:mm:ss}");
            Console.ResetColor();

            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Refresh token is missing" });

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.Expires <= DateTime.UtcNow)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($">> REFRESH FAILED: TOKEN IS INVALID OR EXPIRED {timeUtcPlus1:yyyy-MM-dd HH:mm:ss}");
                Console.ResetColor();
                return Unauthorized(new { message = "Refresh token is invalid or expired" });
            }

            // Recupero l'utente collegato al CustomerId
            var userLogin = await _context.UserLogins
                .FirstOrDefaultAsync(u => u.CustomerId == storedToken.CustomerId);

            if (userLogin == null)
            {
                Console.WriteLine("Refresh failed: User not found");
                return Unauthorized(new { message = "User not found" });
            }

            string role = userLogin.Role == 2 ? "Admin" : "User";

            // GenerateJwt usa solo l'Email, la Password non viene usata
            var loginCredentials = new LoginCredentials
            {
                Email = userLogin.Email,
                Password = string.Empty
            };

            var newAccessToken = GenerateJwt(loginCredentials, userLogin.CustomerId, role);

            // Ruoto il refresh token
            var newRefreshToken = GenerateRefreshToken();
            await SaveRefreshTokenToDb(userLogin.CustomerId, newRefreshToken);
            SetRefreshTokenCookie(newRefreshToken);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($">> NEW ACCESS TOKEN SENT TO CLIENT at {timeUtcPlus1:yyyy-MM-dd HH:mm:ss}");
            Console.ResetColor();

            return Ok(new { token = newAccessToken });
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

        private RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                IsRevoked = false,
                TotalRefreshes = 0,
            };
        }

        private void SetRefreshTokenCookie(RefreshToken token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = token.Expires
            };

            Response.Cookies.Append("refreshToken", token.Token, cookieOptions);
        }

        private async Task SaveRefreshTokenToDb(int customerId, RefreshToken newToken)
        {
            var existing = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.CustomerId == customerId);

            if (existing == null)
            {
                newToken.CustomerId = customerId;
                await _context.RefreshTokens.AddAsync(newToken);
            }
            else
            {
                existing.Token = newToken.Token;
                existing.Created = newToken.Created;
                existing.Expires = newToken.Expires;
                existing.IsRevoked = false;
                existing.ModifiedDate = DateTime.UtcNow;
                existing.TotalRefreshes += 1;

                _context.RefreshTokens.Update(existing);
            }

            await _context.SaveChangesAsync();
        }
    }

}
