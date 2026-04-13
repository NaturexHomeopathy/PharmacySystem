using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Model
{
    public class HealthProductdisease
    {
        [Key]
        public Guid HealthProductDiseaseId { get; set; }
        public Guid DiseaseId { get; set; }
        public Disease Disease { get; set; }    
        public Guid HealthProductId { get; set; }
        public HealthProduct HealthProduct { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
