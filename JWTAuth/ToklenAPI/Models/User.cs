using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToklenAPI.Models
{
    [Table("BasicUser")]
    public class User
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }
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
