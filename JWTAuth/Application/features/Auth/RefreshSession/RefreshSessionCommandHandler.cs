using Application.Interfaces.Auth;
using LanguageExt.Common;
using MediatR;
using ToklenAPI.Models.Dtos.JWTToken;

namespace Application.features.Auth.RefreshSession
{
    public class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand, Result<JWTResult>>
    {
        private readonly IAuthService _authService;

        public RefreshSessionCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result<JWTResult>> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshSessionToken(request.UserId, cancellationToken);
            return new Result<JWTResult>(result);
        }
    }
}
