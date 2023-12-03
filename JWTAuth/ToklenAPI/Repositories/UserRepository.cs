using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ToklenAPI.Data;
using ToklenAPI.Interfaces;
using ToklenAPI.Models;
using ToklenAPI.Models.Dtos;
using ToklenAPI.Models.Dtos.JWTToken;
using ToklenAPI.Models.Session;

namespace ToklenAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly JWTSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpAccesor;

        private string secretKey;

        public UserRepository(ApplicationDbContext bd, IConfiguration configuration, IOptions<JWTSettings> jwtSettings, IHttpContextAccessor httpAccesor)
        {
            _context = bd;
            secretKey = configuration.GetValue<string>("JwtSettings:key");
            _jwtSettings = jwtSettings.Value;
            _httpAccesor = httpAccesor;
        }

        public async Task<string> Register (UserRegisterDto register){
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == register.Email);

            if (user != null)
            {
                throw new BadHttpRequestException("User already exists");
            }
            var salt = GenerateSalt();
            var newUser = new User
            {
                Email = register.Email,
                Fullname = register.Fullname,
                PasswordHash = HashPassword(register.Password, salt),
                Salt = salt
            };
            await _context.Users.AddAsync(newUser);

            var result = await _context.SaveChangesAsync();
            if(result==0)
            {
                throw new BadHttpRequestException("Error registering user");
            }
            return $"user {register.Email} has been created";
        }

        private byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[16];
                rng.GetBytes(salt);
                return salt;
            }
        }

        private string HashPassword(string password, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedPassword = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
                var hash = sha256.ComputeHash(hashedPassword);
                return Convert.ToBase64String(hash);
            }
        }

        public async Task<JWTResult> Login(UserLoginDto login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == login.Email);
            if (user==null) 
            {
                throw new BadHttpRequestException("Check email or password");
            }
            if ( user.PasswordHash != HashPassword(login.Password, user.Salt))
            {
                throw new BadHttpRequestException("Check email or password");
            }
            var jwtResult = GenerateJWTToken(login.Email);
            await GenerateRefreshToken(user);


            return jwtResult;
        }
        public async Task<JWTResult> RefreshSessionToken(int userId)
        {
            //Validations
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                throw new BadHttpRequestException("User doesn't exist");
            }

            var oldRefreshToken = _httpAccesor.HttpContext.Request.Cookies["refreshToken"];
            if (oldRefreshToken == null) { 
                throw new BadHttpRequestException("Problems updating session");
            }

            var storedRefresh = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId);
            if (storedRefresh == null)
            {
                throw new BadHttpRequestException("Problems updating session");
            }
            if(storedRefresh.Revoked != null)
            {
                throw new BadHttpRequestException("Session has been revoked");
            }
            if (storedRefresh.IsExpired)
            {
                throw new BadHttpRequestException("Session has expired");
            }
            var jwtResult = GenerateJWTToken(user.Email);

            var newRefreshToken = new RefreshToken
            {
                Id = new Guid(),
                UserId = userId,
                CreatedByIp = GenerateIpAddress(),
                Expires = DateTime.UtcNow.AddDays(30),
                Token = RandomTokenString(),
            };

            storedRefresh.TokenReplaced = newRefreshToken.Token;
            storedRefresh.Revoked = DateTime.UtcNow;
            storedRefresh.RevokedByIp = GenerateIpAddress();


             _context.RefreshTokens.Update(storedRefresh);
            await _context.AddAsync(newRefreshToken);

            var result = await _context.SaveChangesAsync();
            if (result == 0)
            {
                throw new BadHttpRequestException("Error at updating session, try again");
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires,
                SameSite = SameSiteMode.Strict,
                Secure = true
            };

            _httpAccesor.HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
            return jwtResult;
        }
        private JWTResult GenerateJWTToken(string email)
        {
            //Claims
            var claims = new List<Claim>
            {
                new ("email", email),
                new ("active","true")
            };
            var claimsIdentity = new ClaimsIdentity(claims,"defaultLogin");

            //JWT Configuration
            var expirationDate = DateTime.UtcNow.AddMinutes(_jwtSettings.Duration);
            var symetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signInCredentials = new SigningCredentials(symetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claimsIdentity.Claims,
                expires: expirationDate,
                signingCredentials: signInCredentials
            );

            //JWT Result
            var tokenResult = new JWTResult
            {
                Email = email,
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                JWTExpires = expirationDate,
            };


            return tokenResult;
        }
        private async Task GenerateRefreshToken(User user)
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CreatedByIp = GenerateIpAddress(),
                Expires = DateTime.UtcNow.AddDays(30),
                Token = RandomTokenString(),
            };

            await _context.AddAsync(refreshToken);
            var result = await _context.SaveChangesAsync();
            if(result==0)
            {
                throw new BadHttpRequestException("Error at login, try again");
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.Expires,
                SameSite = SameSiteMode.Strict,
                Secure = true
            };

            _httpAccesor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

        }
        private string GenerateIpAddress()
        {
            var httpContext = _httpAccesor.HttpContext;

            if (httpContext == null)
            {
                return "IP Unavailable";
            }

            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }
            else if (httpContext.Connection.RemoteIpAddress != null)
            {
                return httpContext.Connection.RemoteIpAddress.ToString();
            }

            return "IP Unavailable";
        }
        private string RandomTokenString()
        {
            using var rngCryptoServicesProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServicesProvider.GetBytes(randomBytes);

            return BitConverter.ToString(randomBytes).Replace("-", "");
        }
    }
}
