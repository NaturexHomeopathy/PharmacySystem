using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacySystem.Data;
using PharmacySystem.Enum;
using PharmacySystem.Model;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace PharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtpController : ControllerBase
    {
        private readonly PharmacyContext _context;
        private readonly IConfiguration _config;

        public OtpController(PharmacyContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ================= SEND OTP =================
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] EmailOtp model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest("Email required");

            if (await _context.Users.AnyAsync(x => x.Email == model.Email))
                return BadRequest("Email already registered");

            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var otpHash = HashOtp(otp);

            _context.EmailOtps.Add(new EmailOtp
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                OtpHash = otpHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Attempts = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await SendOtpEmail(
                model.Email,
                "OTP Code",
                $"Your OTP is {otp}"
            );

            return Ok("OTP sent");
        }

        // ================= VERIFY OTP =================
        [HttpPost("verify-otp-register")]
        public async Task<IActionResult> VerifyOtpRegister(
     [FromBody] User user,
     [FromQuery] string otp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user.Email) ||
                    string.IsNullOrWhiteSpace(otp))
                    return BadRequest("Email and OTP required");

                var otpRecord = await _context.EmailOtps
                    .Where(x => x.Email == user.Email && x.IsUsed != true)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otpRecord == null)
                    return BadRequest("OTP not found");

                if (otpRecord.ExpiresAt < DateTime.UtcNow)
                    return BadRequest("OTP expired");

                if (otpRecord.Attempts >= 5)
                    return BadRequest("Too many attempts");

                if (!VerifyHash(otp, otpRecord.OtpHash))
                {
                    otpRecord.Attempts++;
                    await _context.SaveChangesAsync();
                    return BadRequest("Invalid OTP");
                }

                otpRecord.IsUsed = true;

                // IMPORTANT SAFE MAPPING
                var newUser = new User
                {
                    UserId = Guid.NewGuid(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    MobileNumber = user.MobileNumber,
                    Address = user.Address,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash),
                    Role = Role.Visitor,
                    CreatedAt = DateTime.UtcNow,
                    IsEnabled=true
                };

                _context.Users.Add(newUser);

                await _context.SaveChangesAsync();

                return Ok("Registration successful");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        // ================= HELPERS =================
        private static string HashOtp(string otp)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(
                sha.ComputeHash(Encoding.UTF8.GetBytes(otp))
            );
        }

        private static bool VerifyHash(string plainOtp, string storedHash)
        {
            return HashOtp(plainOtp) == storedHash;
        }

        [HttpPost("forgot-send-otp")]
        public async Task<IActionResult> ForgotSendOtp([FromBody] EmailOtp model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest("Email required");

            var userExists = await _context.Users
                .AnyAsync(x => x.Email == model.Email);

            if (!userExists)
                return BadRequest("Email not registered");

            var otp = RandomNumberGenerator
                .GetInt32(100000, 999999).ToString();

            var otpHash = HashOtp(otp);

            _context.EmailOtps.Add(new EmailOtp
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                OtpHash = otpHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Attempts = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await SendOtpEmail(
                model.Email,
                "Password Reset OTP",
                $"Your OTP is {otp}"
            );

            return Ok("OTP sent");
        }

        [HttpPost("forgot-reset-password")]
        public async Task<IActionResult> ForgotResetPassword(
    string email,
    string otp,
    string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(otp) ||
                string.IsNullOrWhiteSpace(newPassword))
                return BadRequest("Email, OTP and password required");

            var otpRecord = await _context.EmailOtps
                .Where(x => x.Email == email && x.IsUsed != true)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
                return BadRequest("OTP not found");

            if (otpRecord.ExpiresAt < DateTime.UtcNow)
                return BadRequest("OTP expired");

            if (otpRecord.Attempts >= 5)
                return BadRequest("Too many attempts");

            if (!VerifyHash(otp, otpRecord.OtpHash))
            {
                otpRecord.Attempts++;
                await _context.SaveChangesAsync();
                return BadRequest("Invalid OTP");
            }

            otpRecord.IsUsed = true;

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return BadRequest("User not found");

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _context.SaveChangesAsync();

            return Ok("Password reset successful");
        }

        private async Task SendOtpEmail(string toEmail, string subject, string body)
        {
            using var smtp = new SmtpClient(
                _config["EmailSettings:SmtpServer"],
                int.Parse(_config["EmailSettings:Port"])
            );

            smtp.Credentials = new NetworkCredential(
                _config["EmailSettings:SmtpLogin"],
                _config["EmailSettings:SmtpPassword"]
            );

            smtp.EnableSsl = true;

            using var message = new MailMessage
            {
                From = new MailAddress(
                    _config["EmailSettings:SenderEmail"],
                    "Pharmacy System"
                ),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            await smtp.SendMailAsync(message);
        }
    }
}
