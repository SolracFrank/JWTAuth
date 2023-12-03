using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ToklenAPI.Models.Dtos
{
    public class UserLoginDto
    {
        [EmailAddress(ErrorMessage = "Email invalid")]
        [StringLength(100)]
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
