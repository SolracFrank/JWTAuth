using ToklenAPI.Models.Dtos;

namespace ToklenAPI.Interfaces
{
    public interface IUserRepository
    {
        public Task<string> Register(UserRegisterDto register);

    }
}
