using Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;
using ToklenAPI.Models.Dtos;

namespace ToklenAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _userRepository;
        public AuthController(IAuthService userRepository)
        {
            _userRepository = userRepository;

        }

        [HttpPost("register")] 
        public async Task<IActionResult> Register([FromBody] UserRegisterDto user,CancellationToken cancellationToken)
        {
            var result = await _userRepository.Register(user, cancellationToken);
            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto user, CancellationToken cancellationToken)
        {
            var result = await _userRepository.Login(user, cancellationToken);
            return Ok(result);
        }

        [HttpPost("refreshsession")]
        public async Task<IActionResult> RefreshSession([FromQuery] int userid, CancellationToken cancellationToken)
        {
            var result = await _userRepository.RefreshSessionToken(userid, cancellationToken);
            return Ok(result);
        }
    }
}
