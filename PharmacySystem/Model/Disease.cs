using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Model
{
    public class Disease
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string DiseaseName { get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? DiseaseUrl { get; set; }
        public Boolean IsActive { get; set; }=true;
    }
}
