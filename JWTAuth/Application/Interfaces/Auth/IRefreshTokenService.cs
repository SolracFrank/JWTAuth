using ToklenAPI.Models;

namespace Application.Interfaces.Auth
{
    public interface IRefreshTokenService
    {
        public Task GenerateRefreshToken(User user, CancellationToken cancellationToken);
        public void RefreshTokenCookies(DateTimeOffset expires, string refreshTokenString);

    }
}
