namespace BankingSystem.Application.DTOs
{
    public class AmountRequest
    {
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}