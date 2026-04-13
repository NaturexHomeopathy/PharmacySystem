using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Model
{
    public class Prescription
    {
        [Key]
        public Guid PrescriptionId { get; set; }

        public Guid AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        public string PrescribedBy { get; set; } // Doctor's name
        public string Notes { get; set; }       // Prescription details
        public DateTime CreatedAt { get; set; }

    }
}
