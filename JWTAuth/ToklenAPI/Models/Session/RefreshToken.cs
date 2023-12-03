using System.ComponentModel.DataAnnotations;

namespace ToklenAPI.Models.Session
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTime Expires { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;

        [Required]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        [Required]
        public string CreatedByIp { get; set; }

        public DateTime? Revoked { get; set; }

        public string RevokedByIp { get; set; }

        public string TokenReplaced { get; set; }

        public bool isActive => Revoked == null && !IsExpired;
    }
}
