using System;

namespace BankingSystem.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }

        public int FromAccountId { get; set; }

        public int ToAccountId { get; set; }

        public decimal Amount { get; set; }

        public string Type { get; set; } = string.Empty; // Debit / Credit

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}