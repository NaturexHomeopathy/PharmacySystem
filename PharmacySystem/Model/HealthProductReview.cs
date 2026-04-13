using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacySystem.Model
{
    public class HealthProductReview
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid HealthProductId { get; set; }

        [ForeignKey("HealthProductId")]
        public HealthProduct HealthProduct { get; set; }

        [Required]
        public Guid UserId { get; set; }   // Assuming you have Users table

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }    // 1 to 5 stars

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}