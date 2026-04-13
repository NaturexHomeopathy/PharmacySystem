using PharmacySystem.Enum;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PharmacySystem.Model
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }

        public Guid? UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }   // ⭐ MUST BE NULLABLE

        public string FullName { get; set; }
        public string Contact { get; set; }
        public string DeliveryAddress { get; set; }

        public decimal? TotalAmount { get; set; }
        public DateTime? CreatedAt { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}