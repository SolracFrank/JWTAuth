using Application.Interfaces.Auth;
using LanguageExt.Common;
using MediatR;
using ToklenAPI.Models.Dtos;

namespace Application.features.Auth.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
    {
        private readonly IAuthService _authService;

        public RegisterCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var regsterDto = new UserRegisterDto
            {
                Fullname = request.Fullname,
                Email = request.Email,
                Password = request.Password,
            };
            var result = await _authService.Register(regsterDto, cancellationToken);

            return new Result<string>(result);
        }
    }
}
