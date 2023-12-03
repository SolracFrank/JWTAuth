using Application.Interfaces.Auth;
using LanguageExt.Common;
using MediatR;
using ToklenAPI.Models.Dtos;
using ToklenAPI.Models.Dtos.JWTToken;

namespace Application.features.Auth.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<JWTResult>>
    {
        private readonly IAuthService _authService;

        public LoginCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result<JWTResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {

            var loginDto = new UserLoginDto
            {
                Email = request.Email,
                Password = request.Password,
            };
            var result = await _authService.Login(loginDto, cancellationToken);

            return new Result<JWTResult>(result);
        }
    }
}
