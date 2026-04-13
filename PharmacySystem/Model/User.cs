using System.ComponentModel.DataAnnotations;
using PharmacySystem.Enum;

namespace PharmacySystem.Model
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? MobileNumber { get; set; }
        public string? Address { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsOnline { get; set; } = false;
        public Role Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

    }
}
