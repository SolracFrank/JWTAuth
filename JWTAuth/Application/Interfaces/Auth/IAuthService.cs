using Domain.Models.Dtos.IdentityDtos;
using ToklenAPI.Models.Dtos;
using ToklenAPI.Models.Dtos.JWTToken;

namespace Application.Interfaces.Auth
{
    public interface IAuthService
    {
        public Task<string> Register(UserRegisterDto register, CancellationToken cancellationToken);
        public Task<JWTResult> Login(UserLoginDto login, CancellationToken cancellationToken);
        public Task<JWTResult> RefreshSessionToken(int userId, CancellationToken cancellationToken);
        public Task<string> IdentityRegister(IdentityUserRegisterDto register, CancellationToken cancellationToken);
        public Task<JWTResult> IdentityLogin(IdentityUserLoginDto login, CancellationToken cancellationToken);



    }
}
