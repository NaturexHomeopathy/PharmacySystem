using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacySystem.Data;
using PharmacySystem.Enum;
using PharmacySystem.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace PharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitorController : ControllerBase
    {
        private readonly PharmacyContext _context;

        public VisitorController(PharmacyContext context)
        {
            _context = context;
        }

        // ✅ ENABLED diseases with images
        [HttpGet("diseases")]
        public async Task<IActionResult> GetEnabledDiseases()
        {
            var diseases = await _context.Diseases
                .Where(d => d.IsActive)
                .Select(d => new
                {
                    diseaseId = d.Id,
                    diseaseName = d.DiseaseName,
                    imageUrl = d.DiseaseUrl
                })
                .OrderBy(d => d.diseaseName)
                .ToListAsync();

            return Ok(diseases);
        }

        // ✅ PRODUCTS BY DISEASE (Visitor shopping)
        [HttpGet("diseases/{diseaseId:guid}/products")]
        public async Task<IActionResult> GetProductsByDisease(Guid diseaseId)
        {
            var products = await _context.HealthProductdiseases
                .Where(x => x.DiseaseId == diseaseId)
                .Include(x => x.HealthProduct)
                .Select(x => x.HealthProduct)
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetActiveCategories()
        {
            var categories = await _context.HealthProductCategories
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    categoryId = c.CategoryId,
                    categoryName = c.CategoryName
                })
                .OrderBy(c => c.categoryName)
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("categories/{categoryId:guid}/products")]
        public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
        {
            var products = await _context.HealthProducts
                .Where(p => p.CategoryId == categoryId)
                .Select(p => new
                {
                    productId = p.HealthProductId,
                    name = p.ProductName,
                    price = p.HealthProductPrice,
                    description = p.ProductDescription,

                    imageUrl = p.Images
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(products);
        }

        // ✅ ALL HEALTH PRODUCTS (Medicines page)
        [HttpGet("healthproducts")]
        public async Task<IActionResult> GetAllHealthProducts()
        {
            var products = await _context.HealthProducts
                .Select(p => new
                {
                    productId = p.HealthProductId,
                    name = p.ProductName,
                    price = p.HealthProductPrice,
                    description = p.ProductDescription,
                    categoryId = p.CategoryId,
                    quantity = p.StockQuantity,
                    imageUrl = p.ImageUrl
                })
                .OrderBy(p => p.name)
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("healthproducts/{id:guid}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var p = await _context.HealthProducts
                .Where(x => x.HealthProductId == id)
                .Select(p => new
                {
                    productId = p.HealthProductId,
                    name = p.ProductName,
                    price = p.HealthProductPrice,
                    description = p.ProductDescription,
                    categoryId = p.CategoryId,
                    quantity = p.StockQuantity,

                    // 🔥 IMPORTANT — IMAGE
                    imageUrl = p.Images
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                        ?? p.ImageUrl
                })
                .FirstOrDefaultAsync();

            if (p == null)
                return NotFound();

            return Ok(p);
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterVisitor([FromBody] User request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(request.PasswordHash))
                return BadRequest("Password is required");

            // OTP SHOULD HAVE CREATED VISITOR USER
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            var mobile = await _context.Users
                .FirstOrDefaultAsync(x => x.MobileNumber == request.MobileNumber);

            var address = await _context.Users
                .FirstOrDefaultAsync(x => x.Address == request.Address);

            if (user == null)
                return BadRequest("Please verify OTP first");

            // ALREADY REGISTERED CHECK
            if (!string.IsNullOrEmpty(user.PasswordHash))
                return BadRequest("User already registered");

            // ENSURE VISITOR ROLE
            if (user.Role != Role.Visitor)
                return BadRequest("Invalid role for visitor registration");

            // SET PASSWORD USING BCRYPT (IMPORTANT)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);

            user.IsEnabled = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Visitor registered successfully"
            });
        }

        [Authorize]
        [HttpPost("appointments")]
        public async Task<IActionResult> BookAppointment([FromBody] Appointment request)
        {
            if (request == null)
                return BadRequest();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            request.BookerId = userId;
            request.Booker = null;

            request.AppointmentStatus = AppointmentStatus.Pending;
            request.CreatedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            _context.Appointments.Add(request);
            await _context.SaveChangesAsync();

            return Ok(request);
        }
        [Authorize]
        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");

            var userId = Guid.Parse(userIdClaim);

            var appointments = await _context.Appointments
                .Where(a => a.BookerId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("users/{userId:guid}/orders")]
        public async Task<IActionResult> GetUserOrders(Guid userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.HealthProduct)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    orderId = o.OrderId,
                    totalAmount = o.TotalAmount,
                    createdAt = o.CreatedAt,
                    status = o.Status.ToString(),
                    items = o.OrderItems.Select(i => new
                    {
                        productId = i.HealthProductId,
                        productName = i.HealthProduct.ProductName,
                        quantity = i.Quantity,
                        price = i.PriceAtPurchase
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpPost("health-products/{productId}/reviews")]
        public async Task<IActionResult> AddReview(Guid productId, [FromBody] HealthProductReview review)
        {
            var productExists = await _context.HealthProducts
                .AnyAsync(p => p.HealthProductId == productId);

            if (!productExists)
                return NotFound("Product not found");

            review.Id = Guid.NewGuid();
            review.HealthProductId = productId;
            review.CreatedAt = DateTime.UtcNow;

            _context.HealthProductReviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(review);
        }

        [HttpPut("appointments/{id}/visitor-cancel")]
        public async Task<IActionResult> VisitorCancelAppointment(int id, Guid visitorId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.BookerId == visitorId);

            if (appointment == null)
                return NotFound("Appointment not found");

            if (appointment.AppointmentStatus != AppointmentStatus.Pending)
                return BadRequest("Appointment already processed");

            var hoursLeft = (appointment.AppointmentDate - DateTime.UtcNow).TotalHours;

            if (hoursLeft < 6)
                return BadRequest("Appointment cannot be cancelled within 6 hours");

            appointment.AppointmentStatus = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Appointment cancelled successfully");
        }

        [Authorize] // ⭐ Require login
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            if (order == null || order.OrderItems == null || order.OrderItems.Count == 0)
                return BadRequest("Invalid order");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ⭐ Server-side values
                order.OrderId = Guid.NewGuid();
                var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                order.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);
                order.Status = OrderStatus.Pending;

                // =========================================================
                // ⭐ CORRECT USER ID FROM JWT TOKEN
                // =========================================================

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("User ID not found in token");

                order.UserId = Guid.Parse(userIdClaim);

                // =========================================================

                decimal totalAmount = 0;

                foreach (var item in order.OrderItems)
                {
                    if (item.Quantity <= 0)
                        continue;

                    var product = await _context.HealthProducts
                        .FindAsync(item.HealthProductId);

                    if (product == null)
                        continue;

                    item.OrderItemId = Guid.NewGuid();
                    item.OrderId = order.OrderId;

                    // ⭐ Always take price from DB
                    item.PriceAtPurchase = product.HealthProductPrice;

                    totalAmount += (item.PriceAtPurchase ?? 0) * item.Quantity;
                }

                if (totalAmount == 0)
                    return BadRequest("No valid items");

                order.TotalAmount = totalAmount;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Order placed successfully",
                    orderId = order.OrderId,
                    totalAmount = order.TotalAmount,
                    status = order.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    orderId = o.OrderId,
                    fullName = o.FullName,
                    contact = o.Contact,
                    deliveryAddress = o.DeliveryAddress,
                    totalAmount = o.TotalAmount,
                    status = o.Status.ToString(),   // ⭐ IMPORTANT
                    createdAt = o.CreatedAt,
                    items = o.OrderItems.Select(i => new
                    {
                        productId = i.HealthProductId,
                        productName = i.HealthProduct.ProductName, // ⭐ IMPORTANT
                        quantity = i.Quantity,
                        price = i.PriceAtPurchase
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }
    }
}
