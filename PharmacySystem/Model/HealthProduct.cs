using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using PharmacySystem.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class HealthProduct
{
    [Key]
    public Guid HealthProductId { get; set; }

    [Required]
    public string ProductName { get; set; }

    public string? ProductDescription { get; set; }

    [Required]
    public Guid? CategoryId { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public HealthProductCategory Category { get; set; }

    public string? Brand { get; set; }

    [Required]
    public decimal HealthProductPrice { get; set; }

    public int StockQuantity { get; set; }

    public string ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ValidateNever]
    public ICollection<HealthProductImage> Images { get; set; } = new List<HealthProductImage>();

    [ValidateNever]
    public ICollection<HealthProductReview> Reviews { get; set; } = new List<HealthProductReview>();
}