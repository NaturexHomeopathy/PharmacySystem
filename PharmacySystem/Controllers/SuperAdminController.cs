using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacySystem.Data;
using PharmacySystem.Enum;
using PharmacySystem.Model;

namespace PharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuperAdminController : ControllerBase
    {
        private readonly PharmacyContext _context;
        private readonly IConfiguration _configuration;

        public SuperAdminController(PharmacyContext context, IConfiguration _configuration)
        {
            _context = context;
            this._configuration = _configuration;
        }

        // ================= USER / ROLE MANAGEMENT =================

        [HttpGet("superadmins")]
        public async Task<IActionResult> GetSuperAdmins()
        {
            return Ok(await _context.Users
                .Where(u => u.Role == Enum.Role.SuperAdmin)
                .ToListAsync());
        }

        [HttpPost("superadmins")]
        public async Task<IActionResult> AddSuperAdmin([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                return BadRequest("Password is required");

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest("User already exists");

            var superAdmin = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                Address = user.Address,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash),
                Role = Enum.Role.SuperAdmin,
                IsEnabled = true,
                IsOnline = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(superAdmin);
            await _context.SaveChangesAsync();

            return Ok(superAdmin);
        }

        [HttpPut("superadmins/{id:guid}")]
        public async Task<IActionResult> UpdateSuperAdmin(Guid id, [FromBody] User user)
        {
            var existing = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id && u.Role == Enum.Role.SuperAdmin);

            if (existing == null)
                return NotFound("SuperAdmin not found");

            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.Email = user.Email;
            existing.MobileNumber = user.MobileNumber;
            existing.Address = user.Address;
            // ✅ Update password only if provided
            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPut("users/{id:guid}/enable")]
        public async Task<IActionResult> EnableUser(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            user.IsEnabled = true;
            await _context.SaveChangesAsync();
            return Ok(true);
        }

        [HttpPut("users/{id:guid}/disable")]
        public async Task<IActionResult> DisableUser(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            user.IsEnabled = false;
            user.IsOnline = false;
            await _context.SaveChangesAsync();
            return Ok(true);
        }

        // ================= ADMINS =================

        [HttpGet("admins")]
        public async Task<IActionResult> GetAdmins()
        {
            return Ok(await _context.Users
                .Where(u => u.Role == Enum.Role.Admin)
                .ToListAsync());
        }

        [HttpPost("admins")]
        public async Task<IActionResult> AddAdmin([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                return BadRequest("Password is required");

            var admin = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                MobileNumber= user.MobileNumber,
                Address= user.Address,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash),
                Role = Enum.Role.Admin,
                IsEnabled = true,
                IsOnline = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();
            return Ok(admin);
        }

        [HttpPut("admins/{id:guid}")]
        public async Task<IActionResult> UpdateAdmin(Guid id, [FromBody] User user)
        {
            var existing = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id && u.Role == Enum.Role.Admin);

            if (existing == null)
                return NotFound("Admin not found");

            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.Email = user.Email;
            existing.MobileNumber = user.MobileNumber;
            existing.Address = user.Address;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // ================= VISITORS =================

        [HttpGet("visitors")]
        public async Task<IActionResult> GetVisitors()
        {
            return Ok(await _context.Users
                .Where(u => u.Role == Enum.Role.Visitor)
                .ToListAsync());
        }

        // ================= ONLINE / OFFLINE =================

        [Authorize]
        [HttpPost("online")]
        public async Task<IActionResult> SetOnline([FromBody] dynamic body)
        {
            if (body?.userId == null)
                return BadRequest("UserId is required");

            Guid userId = Guid.Parse(body.userId.ToString());

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null || !user.IsEnabled)
                return BadRequest("User not found or disabled");

            user.IsOnline = true;
            await _context.SaveChangesAsync();
            return Ok(true);
        }

        [Authorize]
        [HttpPost("offline")]
        public async Task<IActionResult> SetOffline([FromBody] dynamic body)
        {
            if (body?.userId == null)
                return BadRequest("UserId is required");

            Guid userId = Guid.Parse(body.userId.ToString());

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return BadRequest("User not found");

            user.IsOnline = false;
            await _context.SaveChangesAsync();
            return Ok(true);
        }

        // ================= HEALTH PRODUCTS =================

        [HttpPost("health-products")]
        public async Task<IActionResult> AddHealthProduct([FromBody] HealthProduct product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            product.HealthProductId = Guid.NewGuid();
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            // 👇 CategoryId is ALREADY set from Angular
            _context.HealthProducts.Add(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }


        [HttpGet("health-products/category/{categoryId:guid}")]
        public async Task<IActionResult> GetHealthProductsByCategory(Guid categoryId)
        {
            var products = await _context.HealthProducts
                .Where(h => h.CategoryId == categoryId)
                .ToListAsync();

            return Ok(products);
        }



        [HttpGet("health-products/all")]
        public async Task<IActionResult> GetAllHealthProducts()
        {
            var products = await _context.HealthProducts
                .AsNoTracking()                 // improves performance
                .OrderBy(p => p.ProductName)    // optional: alphabetical
                .ToListAsync();

            return Ok(products);
        }



        [HttpPut("health-products/{id:guid}")]
        public async Task<IActionResult> UpdateHealthProduct(Guid id, [FromBody] HealthProduct updatedProduct)
        {
            var existing = await _context.HealthProducts
                .FirstOrDefaultAsync(h => h.HealthProductId == id);

            if (existing == null)
                return NotFound("Health product not found");

            // ✅ Update fields
            existing.ProductName = updatedProduct.ProductName;
            existing.ProductDescription = updatedProduct.ProductDescription;
            existing.Category = updatedProduct.Category;
            existing.Brand = updatedProduct.Brand;
            existing.HealthProductPrice = updatedProduct.HealthProductPrice;
            existing.StockQuantity = updatedProduct.StockQuantity;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }


        [HttpDelete("health-products/{id:guid}")]
        public async Task<IActionResult> DeleteHealthProduct(Guid id)
        {
            var product = await _context.HealthProducts.FirstOrDefaultAsync(h => h.HealthProductId == id);
            if (product == null) return NotFound();

            _context.HealthProducts.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //Categories

        [HttpPost("categories")]
        public async Task<IActionResult> AddCategory([FromBody] HealthProductCategory category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                return BadRequest("Category name is required");

            var exists = await _context.HealthProductCategories
                .AnyAsync(c => c.CategoryName == category.CategoryName);

            if (exists)
                return BadRequest("Category already exists");

            category.CategoryId = Guid.NewGuid();
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            category.IsActive = true;

            _context.HealthProductCategories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(category);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.HealthProductCategories
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPut("categories/{id:guid}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] HealthProductCategory updatedCategory)
        {
            var category = await _context.HealthProductCategories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound("Category not found");

            category.CategoryName = updatedCategory.CategoryName;
            category.IsActive = updatedCategory.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [HttpPut("categories/{id:guid}/toggle")]
        public async Task<IActionResult> ToggleCategory(Guid id)
        {
            var category = await _context.HealthProductCategories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound("Category not found");

            category.IsActive = !category.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(category);
        }

        // ================= DISEASE MANAGEMENT =================

        // ADD DISEASE
        [HttpPost("diseases")]
        public async Task<IActionResult> AddDisease([FromBody] Disease disease)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool exists = await _context.Diseases
                .AnyAsync(d => d.DiseaseName.ToLower() == disease.DiseaseName.ToLower());

            if (exists)
                return BadRequest("Disease already exists");

            disease.Id = Guid.NewGuid();
            disease.CreatedAt = DateTime.UtcNow;
            disease.UpdatedAt = DateTime.UtcNow;
            disease.IsActive = true;

            _context.Diseases.Add(disease);
            await _context.SaveChangesAsync();

            return Ok(disease);
        }

        // GET ALL DISEASES
        [HttpGet("diseases")]
        public async Task<IActionResult> GetAllDiseases()
        {
            var diseases = await _context.Diseases
                .AsNoTracking()
                .OrderBy(d => d.DiseaseName)
                .ToListAsync();

            return Ok(diseases);
        }

        // UPDATE DISEASE
        [HttpPut("diseases/{id:guid}")]
        public async Task<IActionResult> UpdateDisease(Guid id, [FromBody] Disease updatedDisease)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var disease = await _context.Diseases.FirstOrDefaultAsync(d => d.Id == id);
            if (disease == null)
                return NotFound("Disease not found");

            bool exists = await _context.Diseases
                .AnyAsync(d => d.Id != id &&
                               d.DiseaseName.ToLower() == updatedDisease.DiseaseName.ToLower());

            if (exists)
                return BadRequest("Disease name already exists");

            disease.DiseaseName = updatedDisease.DiseaseName;
            disease.IsActive = updatedDisease.IsActive;
            disease.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(disease);
        }

        [HttpPut("diseases/{id:guid}/toggle")]
        public async Task<IActionResult> ToggleDisease(Guid id)
        {
            var disease = await _context.Diseases.FirstOrDefaultAsync(d => d.Id == id);
            if (disease == null)
                return NotFound("Disease not found");

            disease.IsActive = !disease.IsActive;
            disease.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(disease);
        }


        // DELETE DISEASE (HARD DELETE)
        [HttpDelete("diseases/{id:guid}")]
        public async Task<IActionResult> DeleteDisease(Guid id)
        {
            var disease = await _context.Diseases.FirstOrDefaultAsync(d => d.Id == id);
            if (disease == null)
                return NotFound("Disease not found");

            _context.Diseases.Remove(disease);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ================= DISEASE ↔ HEALTH PRODUCT =================

        // GET products for a disease
        [HttpGet("diseases/{diseaseId:guid}/products")]
        public async Task<IActionResult> GetProductsForDisease(Guid diseaseId)
        {
            var products = await _context.HealthProductdiseases
                .Where(x => x.DiseaseId == diseaseId)
                .Include(x => x.HealthProduct)
                .Select(x => x.HealthProduct)
                .ToListAsync();

            return Ok(products);
        }

        // ADD product to disease
        [HttpPost("diseases/{diseaseId:guid}/products/{productId:guid}")]
        public async Task<IActionResult> AddProductToDisease(Guid diseaseId, Guid productId)
        {
            bool exists = await _context.HealthProductdiseases.AnyAsync(x =>
                x.DiseaseId == diseaseId && x.HealthProductId == productId);

            if (exists)
                return BadRequest("Product already linked to disease");

            var link = new HealthProductdisease
            {
                HealthProductDiseaseId = Guid.NewGuid(),
                DiseaseId = diseaseId,
                HealthProductId = productId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.HealthProductdiseases.Add(link);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // REMOVE product from disease
        [HttpDelete("diseases/{diseaseId:guid}/products/{productId:guid}")]
        public async Task<IActionResult> RemoveProductFromDisease(Guid diseaseId, Guid productId)
        {
            var link = await _context.HealthProductdiseases.FirstOrDefaultAsync(x =>
                x.DiseaseId == diseaseId && x.HealthProductId == productId);

            if (link == null)
                return NotFound();

            _context.HealthProductdiseases.Remove(link);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        
        [HttpPost("health-products/{id:guid}/upload-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadHealthProductImage(Guid id, IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Image is missing or empty");

            if (image.Length > 5 * 1024 * 1024)
                return BadRequest("Max file size is 5MB");

            var product = await _context.HealthProducts
                .FirstOrDefaultAsync(h => h.HealthProductId == id);

            if (product == null)
                return NotFound("Health product not found");

            var account = new Account(
                _configuration["Cloudinary:CloudName"],
                _configuration["Cloudinary:ApiKey"],
                _configuration["Cloudinary:ApiSecret"]);

            var cloudinary = new Cloudinary(account);

            ImageUploadResult uploadResult;

            using (var stream = image.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(image.FileName, stream),
                    Folder = "health-products",
                    Transformation = new Transformation()
                        .Width(1500)
                        .Height(1500)
                        .Crop("limit")
                        .Quality("auto")
                        .FetchFormat("auto")
                };

                uploadResult = await cloudinary.UploadAsync(uploadParams);
            }

            if (uploadResult?.SecureUrl == null)
                return BadRequest(uploadResult?.Error?.Message ?? "Upload failed");

            // Save image URL to product
            product.ImageUrl = uploadResult.SecureUrl.ToString();
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                productId = product.HealthProductId,
                imageUrl = product.ImageUrl
            });
        }

        [HttpPost("diseases/{id:guid}/upload-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDiseaseImage(Guid id, IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Image missing");

            var disease = await _context.Diseases.FirstOrDefaultAsync(d => d.Id == id);
            if (disease == null)
                return NotFound("Disease not found");

            var account = new Account(
                _configuration["Cloudinary:CloudName"],
                _configuration["Cloudinary:ApiKey"],
                _configuration["Cloudinary:ApiSecret"]
            );

            var cloudinary = new Cloudinary(account);

            ImageUploadResult uploadResult;
            using (var stream = image.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(image.FileName, stream),
                    Folder = "diseases" // 📁 Cloudinary folder
                };

                uploadResult = await cloudinary.UploadAsync(uploadParams);
            }

            disease.DiseaseUrl = uploadResult.SecureUrl.ToString();
            disease.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { imageUrl = disease.DiseaseUrl });
        }

        [HttpPut("orders/{orderId:guid}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
                return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order status updated" });
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders(
    DateTime? fromDate,
    DateTime? toDate,
    OrderStatus? status)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.HealthProduct)
                .AsQueryable();

            // 🔹 Date filter
            if (fromDate.HasValue)
                query = query.Where(o => o.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.CreatedAt <= toDate.Value.AddDays(1));

            // 🔹 Status filter
            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            var orders = await query
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    orderId = o.OrderId,
                    fullName = o.FullName,
                    email = o.User != null ? o.User.Email : null, // 👈 ADD THIS
                    contact = o.Contact,
                    address = o.DeliveryAddress,
                    totalAmount = o.TotalAmount,
                    status = o.Status,
                    createdAt = o.CreatedAt,

                    items = o.OrderItems.Select(i => new
                    {
                        productName = i.HealthProduct.ProductName,
                        quantity = i.Quantity,
                        price = i.PriceAtPurchase
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments(
     [FromQuery] DateTime? fromDate,
     [FromQuery] DateTime? toDate,
     [FromQuery] AppointmentStatus? status)
        {
            var query = _context.Appointments.AsQueryable();

            if (fromDate != null)
            {
                var startDate = fromDate.Value.Date;
                query = query.Where(a => a.AppointmentDate >= startDate);
            }

            if (toDate != null)
            {
                var endDate = toDate.Value.Date.AddDays(1);
                query = query.Where(a => a.AppointmentDate < endDate);
            }

            if (status != null)
            {
                query = query.Where(a => a.AppointmentStatus == status);
            }

            var appointments = await query
                .Include(a=>a.Booker)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return Ok(appointments);
        }
        [HttpPut("appointments/{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, AppointmentStatus status)
        {
            if (status != AppointmentStatus.Cancelled && status != AppointmentStatus.Completed)
                return BadRequest("Status can only be Cancelled or Completed");

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound("Appointment not found");

            if (appointment.AppointmentStatus != AppointmentStatus.Pending)
                return BadRequest("Appointment status already updated");

            appointment.AppointmentStatus = status;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(appointment);
        }
    }
}

