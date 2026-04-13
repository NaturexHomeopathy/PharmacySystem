using Microsoft.EntityFrameworkCore;
using PharmacySystem.Model;
using PharmacySystem.Enum;
namespace PharmacySystem.Data
{
    public class PharmacyContext : DbContext
    {
        public PharmacyContext(DbContextOptions<PharmacyContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<HealthProduct> HealthProducts { get; set; }
        public DbSet<Disease> Diseases { get; set; }
        public DbSet<HealthProductdisease> HealthProductdiseases { get; set; }
        public DbSet<HealthProductCategory> HealthProductCategories { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<EmailOtp> EmailOtps { get; set; }
        public DbSet<HealthProductImage> HealthProductImages { get; set; }
        public DbSet<HealthProductReview> HealthProductReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //For Enum Int Values To Be Seen As Original Ones In Database

            modelBuilder.Entity<User>() .HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<string>();

            modelBuilder.Entity<Transaction>().Property(t => t.PaymentMethod).HasConversion<string>();

            modelBuilder.Entity<Transaction>().Property(t => t.TransactionStatus).HasConversion<string>();

            modelBuilder.Entity<Appointment>().Property(p=>p.AppointmentStatus).HasConversion<string>();

            modelBuilder.Entity<HealthProductdisease>()
    .HasOne(hpd => hpd.Disease)
    .WithMany()
    .HasForeignKey(hpd => hpd.DiseaseId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HealthProductdisease>()
                .HasOne(hpd => hpd.HealthProduct)
                .WithMany()
                .HasForeignKey(hpd => hpd.HealthProductId)
                .OnDelete(DeleteBehavior.Cascade);


            //To seed the data

            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = Guid.NewGuid(),
                FirstName = "Rohit",
                LastName = "Bargode",
                Email = "Rohit@123",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Rohit123"),
                Role = Role.SuperAdmin,
                CreatedAt = DateTime.UtcNow,
                IsEnabled=true
            });


        }
    }
}
