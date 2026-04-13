using PharmacySystem.Enum;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace PharmacySystem.Model
{
    public class Transaction
    {
        [Key]
        public Guid TransactionId { get; set; }

        // Who paid
        public Guid UserId { get; set; }
        public User User { get; set; }

        // Optional link to appointment (if payment is for consultation)
        public Guid? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public TransactionStatuss TransactionStatus { get; set; }

        public string? ReferenceNumber { get; set; }   // e.g. UPI ID, gateway ref

        public DateTime CreatedAt { get; set; }
    }
}
