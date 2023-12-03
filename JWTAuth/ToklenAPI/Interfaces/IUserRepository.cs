using ToklenAPI.Models.Dtos;
using ToklenAPI.Models.Dtos.JWTToken;

namespace ToklenAPI.Interfaces
{
    public interface IUserRepository
    {
        public Task<string> Register(UserRegisterDto register);
        public Task<JWTResult> Login(UserLoginDto login);

    }
}
