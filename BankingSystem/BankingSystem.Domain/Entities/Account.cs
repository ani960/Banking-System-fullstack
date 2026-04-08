namespace BankingSystem.Domain.Entities
{
    public class Account
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string AccountNumber { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public string AccountType { get; set; } = string.Empty;

        // Navigation Property
        public User? User { get; set; }
    }
}