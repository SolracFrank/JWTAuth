using Application.features.Auth.Login;
using Application.features.Auth.RefreshSession;
using Application.features.Auth.Register;
using Microsoft.AspNetCore.Mvc;

namespace ToklenAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {

        [HttpPost("register")] 
        public async Task<IActionResult> Register([FromBody] RegisterCommand request,CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new RegisterCommand
            {
                Email = request.Email,
                Fullname = request.Fullname,
                Password = request.Password,
            }, cancellationToken);

            return result.ToOk();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand request, CancellationToken cancellationToken)
        {
           var result = await Mediator.Send(new LoginCommand { Email = request.Email,Password = request.Password },cancellationToken);

            return result.ToOk();
        }

        [HttpPost("refreshsession")]
        public async Task<IActionResult> RefreshSession([FromQuery] RefreshSessionCommand request, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new RefreshSessionCommand { UserId = request.UserId},cancellationToken);
            return result.ToOk();
        }
    }
}
