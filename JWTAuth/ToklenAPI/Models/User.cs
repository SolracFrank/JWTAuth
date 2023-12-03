using System.ComponentModel.DataAnnotations;

namespace ToklenAPI.Models
{
    public class User
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Fullname { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Email invalid")]
        [StringLength(100)]

        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+\-=\[\]{};':\\|,.<>\/?]).{6,}$", ErrorMessage = "Password should have: alteast min 6 characters, one uppercase, one number and one especial character")]
        public string Password { get; set; }
    }
}
