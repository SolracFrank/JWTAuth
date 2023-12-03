using Application.Interfaces.AppServices;
using Application.Interfaces.Auth;
using Domain.Interfaces;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using ToklenAPI.Models;
using ToklenAPI.Models.Session;

namespace Infrastructure.Services.Auth
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IHttpContextAccessor _httpAccesor;
        private readonly IIpAddressAccesorService _ipAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenService(IHttpContextAccessor httpAccesor, IIpAddressAccesorService ipAccessor, IUnitOfWork unitOfWork)
        {
            _httpAccesor = httpAccesor;
            _ipAccessor = ipAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task GenerateRefreshToken(User user, CancellationToken cancellationToken)
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CreatedByIp = _ipAccessor.GenerateIpAddress(),
                Expires = DateTime.UtcNow.AddDays(30),
                Token = RefreshTokenStringGenerator.RandomTokenString(),
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (!result)
            {
                throw new BadHttpRequestException("Error at login, try again");
            }

            RefreshTokenCookies(refreshToken.Expires, refreshToken.Token);
        }

        public void RefreshTokenCookies(DateTimeOffset expires, string refreshTokenString)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires,
                SameSite = SameSiteMode.Strict,
                Secure = true
            };

            _httpAccesor.HttpContext.Response.Cookies.Append("refreshToken", refreshTokenString, cookieOptions);
        }
    }
}
