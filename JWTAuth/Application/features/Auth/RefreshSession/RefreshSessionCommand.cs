using LanguageExt.Common;
using MediatR;
using ToklenAPI.Models.Dtos.JWTToken;

namespace Application.features.Auth.RefreshSession
{
    public class RefreshSessionCommand : IRequest<Result<JWTResult>>
    {
        public int UserId { get; set; }
    }
}
