using Application.Interfaces.AppServices;
using Application.Interfaces.Auth;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ToklenAPI.Models;
using ToklenAPI.Models.Dtos;
using ToklenAPI.Models.Dtos.JWTToken;
using ToklenAPI.Models.Session;

namespace Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly JWTSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpAccesor;
        private readonly IIpAddressAccesorService _ipAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IConfiguration configuration, IOptions<JWTSettings> jwtSettings, IHttpContextAccessor httpAccesor, IIpAddressAccesorService ipAccessor, IUnitOfWork unitOfWork)
        {
            _jwtSettings = jwtSettings.Value;
            _httpAccesor = httpAccesor;
            _ipAccessor = ipAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> Register(UserRegisterDto register, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.BasicUsers.FirstOrDefaultAsync(x => x.Email == register.Email, cancellationToken);

            if (user != null)
            {
                throw new BadRequestException("User already exists");
            }
            var salt = PasswordHasher.GenerateSalt();
            var newUser = new User
            {
                Email = register.Email,
                Fullname = register.Fullname,
                PasswordHash = PasswordHasher.HashPassword(register.Password, salt),
                Salt = salt
            };
            await _unitOfWork.BasicUsers.AddAsync(newUser,cancellationToken);

            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (!result)
            {
                throw new BadRequestException("Error registering user");
            }
            return $"user {register.Email} has been created";
        }

        public async Task<JWTResult> Login(UserLoginDto login, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.BasicUsers.FirstOrDefaultAsync(x => x.Email == login.Email,cancellationToken);
            if (user == null)
            {
                throw new BadRequestException("Check email or password");
            }
            if (user.PasswordHash != PasswordHasher.HashPassword(login.Password, user.Salt))
            {
                throw new BadRequestException("Check email or password");
            }
            var jwtResult = GenerateJWTToken(login.Email);
            await GenerateRefreshToken(user,cancellationToken);


            return jwtResult;
        }
        public async Task<JWTResult> RefreshSessionToken(int userId, CancellationToken cancellationToken)
        {
            //Validations
            var user = await _unitOfWork.BasicUsers.FirstOrDefaultAsync(x => x.Id == userId,cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("User doesn't exist");
            }

            var oldRefreshToken = _httpAccesor.HttpContext.Request.Cookies["refreshToken"];
            if (oldRefreshToken == null)
            {
                throw new BadRequestException("Problems updating session");
            }

            var storedRefresh = await _unitOfWork.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId,cancellationToken);
            if (storedRefresh == null)
            {
                throw new BadRequestException("Problems updating session");
            }
            if (storedRefresh.Revoked != null)
            {
                throw new BadRequestException("Session has been revoked");
            }
            if (storedRefresh.IsExpired)
            {
                throw new NotFoundException("Session has expired");
            }
            var jwtResult = GenerateJWTToken(user.Email);

            var newRefreshToken = new RefreshToken
            {
                Id = new Guid(),
                UserId = userId,
                CreatedByIp = _ipAccessor.GenerateIpAddress(),
                Expires = DateTime.UtcNow.AddDays(30),
                Token = RefreshTokenStringGenerator.RandomTokenString(),
            };

            storedRefresh.TokenReplaced = newRefreshToken.Token;
            storedRefresh.Revoked = DateTime.UtcNow;
            storedRefresh.RevokedByIp = _ipAccessor.GenerateIpAddress();


            _unitOfWork.RefreshTokens.Update(storedRefresh);
            await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken,cancellationToken);

            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (!result)
            {
                throw new BadRequestException("Error at updating session, try again");
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
            var claimsIdentity = new ClaimsIdentity(claims, "defaultLogin");

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
        private async Task GenerateRefreshToken(User user, CancellationToken cancellationToken)
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CreatedByIp = _ipAccessor.GenerateIpAddress(),
                Expires = DateTime.UtcNow.AddDays(30),
                Token = RefreshTokenStringGenerator.RandomTokenString(),
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken,cancellationToken);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (!result)
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
       
    
    }
}
