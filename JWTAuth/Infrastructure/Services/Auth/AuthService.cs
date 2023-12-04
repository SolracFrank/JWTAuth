using Application.Interfaces.AppServices;
using Application.Interfaces.Auth;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models.Dtos.IdentityDtos;
using Infrastructure.CustomEntities;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ToklenAPI.Models;
using ToklenAPI.Models.Dtos;
using ToklenAPI.Models.Dtos.JWTToken;
using ToklenAPI.Models.Session;

namespace Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly JWTSettings _jwtSettings;
        private readonly UserManager<AdvancedUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpAccesor;
        private readonly IIpAddressAccesorService _ipAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthService(IConfiguration configuration, IOptions<JWTSettings> jwtSettings, IHttpContextAccessor httpAccesor, IIpAddressAccesorService ipAccessor, IUnitOfWork unitOfWork, IRefreshTokenService refreshTokenService, UserManager<AdvancedUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _jwtSettings = jwtSettings.Value;
            _httpAccesor = httpAccesor;
            _ipAccessor = ipAccessor;
            _unitOfWork = unitOfWork;
            _refreshTokenService = refreshTokenService;
            _userManager = userManager;
            _roleManager = roleManager;
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
            var jwtResult = JwtTokenGenerator.GenerateJWTToken(login.Email,_jwtSettings);
            await _refreshTokenService.GenerateRefreshToken(user.Id.ToString(),cancellationToken);


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

            var storedRefresh = await _unitOfWork.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId.ToString(),cancellationToken);
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
            var jwtResult = JwtTokenGenerator.GenerateJWTToken(user.Email,_jwtSettings);

            var newRefreshToken = new RefreshToken
            {
                Id = new Guid(),
                UserId = userId.ToString(),
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

            _refreshTokenService.RefreshTokenCookies(newRefreshToken.Expires, newRefreshToken.Token);
            return jwtResult;
        }

        public async Task<string> IdentityRegister(IdentityUserRegisterDto register, CancellationToken cancellationToken)
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await _roleManager.RoleExistsAsync("Basic"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Basic"));
            }

            var user = await _userManager.FindByEmailAsync(register.Email);

            if (user != null)
            {
                throw new BadRequestException("User already exists");
            }
            user.Email = register.Email; user.UserName = register.UserName; user.BirthDate = register.BirthDate;

            var result = await _userManager.CreateAsync(user, register.Password);

            if (result.Succeeded)
            {

                await _userManager.AddToRoleAsync(user, "Basic");

                return "User registered succesfully";
            }
            else if (result.Errors.Count() > 0)
            {
                throw new BadRequestException("Error registering user");

            }
            throw new BadRequestException("Unexpected Error");
        }

        public async Task<JWTResult> IdentityLogin(IdentityUserLoginDto login, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user == null)
            {
                throw new BadRequestException("Check email or password");
            }
            var validPassowrd = await _userManager.CheckPasswordAsync(user,login.Password);

            if (!validPassowrd)
            {
                throw new BadRequestException("Check email or password");
            }

            var jwtResult = JwtTokenGenerator.GenerateJWTToken(login.Email, _jwtSettings);
            await _refreshTokenService.GenerateRefreshToken(user.Id, cancellationToken);


            return jwtResult;
        }
    }
}
