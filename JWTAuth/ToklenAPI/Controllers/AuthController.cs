using Microsoft.AspNetCore.Mvc;
using ToklenAPI.Interfaces;
using ToklenAPI.Models.Dtos;

namespace ToklenAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly IUserRepository _userRepository;

        public AuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("register")] 
        public async Task<IActionResult> Register([FromBody] UserRegisterDto user)
        {
            var result = await _userRepository.Register(user);
            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto user)
        {
            var result = await _userRepository.Login(user);
            return Ok(result);
        }

        [HttpPost("refreshsession")]
        public async Task<IActionResult> RefreshSession([FromQuery] int userid)
        {
            var result = await _userRepository.RefreshSessionToken(userid);
            return Ok(result);
        }
    }
}
