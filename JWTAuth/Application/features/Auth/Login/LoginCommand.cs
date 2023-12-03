using LanguageExt.Common;
using MediatR;
using ToklenAPI.Models.Dtos.JWTToken;

namespace Application.features.Auth.Login
{
    public class LoginCommand : IRequest<Result<JWTResult>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
