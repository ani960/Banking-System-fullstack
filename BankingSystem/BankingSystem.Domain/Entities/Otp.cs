namespace BankingSystem.Domain.Entities
{
    public class Otp
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public DateTime ExpiryTime { get; set; }
    }
}