using LanguageExt.Common;
using MediatR;

namespace Application.features.Auth.Register
{
    public class RegisterCommand : IRequest<Result<string>>
    {
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
