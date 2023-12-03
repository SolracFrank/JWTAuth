using Microsoft.AspNetCore.Mvc;
using ToklenAPI.Interfaces;
using ToklenAPI.Models.Dtos;

namespace ToklenAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
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

    }
}
