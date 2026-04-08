using System.Collections.Generic;

namespace BankingSystem.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        // Navigation Property
        public List<Account> Accounts { get; set; } = new();
    }
}