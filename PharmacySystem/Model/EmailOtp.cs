namespace PharmacySystem.Model
{
    public class EmailOtp
    {
        public Guid? Id { get; set; }
        public string? Email { get; set; }
        public string? OtpHash { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool? IsUsed { get; set; }
        public int? Attempts { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

}
