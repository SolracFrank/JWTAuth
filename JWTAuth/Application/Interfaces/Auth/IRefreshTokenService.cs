namespace Application.Interfaces.Auth
{
    public interface IRefreshTokenService
    {
        public Task GenerateRefreshToken(string user, CancellationToken cancellationToken);
        public void RefreshTokenCookies(DateTimeOffset expires, string refreshTokenString);

    }
}
