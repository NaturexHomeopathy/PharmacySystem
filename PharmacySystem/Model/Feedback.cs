using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Model
{
    public class Feedback
    {
        [Key]
        public Guid FeedbackId { get; set; }
        public Guid? UserId { get; set; }
        public User User { get; set; }
        public Guid? AppintmentId { get; set; }
        public Appointment Appointment { get; set; }
        public int? Rating { get; set; } // e.g. 1 to 5
        public string? Comments { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
