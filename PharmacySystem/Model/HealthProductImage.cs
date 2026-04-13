using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacySystem.Model
{
    public class HealthProductImage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid HealthProductId { get; set; }

        [ForeignKey("HealthProductId")]
        public HealthProduct HealthProduct { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        // IMPORTANT for Cloudinary deletion
        [Required]
        public string PublicId { get; set; }

        // Used for reorder panel
        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}