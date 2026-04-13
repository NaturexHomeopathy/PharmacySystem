using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Model
{
    public class HealthProductCategory
    {
        [Key]
        public Guid CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Boolean IsActive { get; set; } = true;
    }
}
