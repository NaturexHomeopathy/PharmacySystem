using System.ComponentModel.DataAnnotations;
using PharmacySystem.Enum;

namespace PharmacySystem.Model
{
    public class Appointment
    {
        [Key]
        public int  AppointmentId { get; set; } // Better to use Guid like UserId

        public Guid BookerId { get; set; }     // FK to User table
        public User? Booker { get; set; }
        public string? BookerName { get; set; }
        public string? BookerContactNumber { get; set; }

        public string? DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }

        public string? AppointmentAddress { get; set; }
        [MaxLength(300)]
        public string? AppointmentReason { get; set; }
        public  AppointmentStatus AppointmentStatus { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
