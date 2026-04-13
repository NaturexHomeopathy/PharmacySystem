using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PharmacySystem.Model
{
    public class OrderItem
    {
        [Key]
        public Guid OrderItemId { get; set; }

        public Guid OrderId { get; set; }

        [JsonIgnore]
        public Order? Order { get; set; }   // ⭐ MUST BE NULLABLE

        public Guid HealthProductId { get; set; }

        [JsonIgnore]
        public HealthProduct? HealthProduct { get; set; }  // ⭐ MUST BE NULLABLE

        public int Quantity { get; set; }

        public decimal? PriceAtPurchase { get; set; }
    }
}