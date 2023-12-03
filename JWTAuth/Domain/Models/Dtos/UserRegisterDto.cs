using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ToklenAPI.Models.Dtos
{
    public class UserRegisterDto
    {
        [Required]
        [StringLength(100)]
        [Column("full_name")]
        public string Fullname { get; set; }
        [Required]
        [Column("email")]

        [EmailAddress(ErrorMessage = "Email invalid")]
        [StringLength(100)]

        public string Email { get; set; }
        [Required]
        [Column("password")]

        [RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+\-=\[\]{};':\\|,.<>\/?]).{6,}$", ErrorMessage = "Password should have: alteast min 6 characters, one uppercase, one number and one especial character")]
        public string Password { get; set; }
    }
}
