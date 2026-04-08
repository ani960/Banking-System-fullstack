using BankingSystem.Application.DTOs;
using BankingSystem.Domain.Entities;
using BankingSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BankingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionController(AppDbContext context)
        {
            _context = context;
        }

        // 🔐 FIXED: Get Current User (by UserId)
        private async Task<User?> GetCurrentUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            var userId = int.Parse(userIdClaim);

            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        // 💸 TRANSFER (FIXED)
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero");

            var user = await GetCurrentUser();
            if (user == null) return Unauthorized("Invalid token");

            var fromAccount = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.AccountNumber == request.FromAccountNumber &&
                    a.UserId == user.Id); // 🔥 SECURITY FIX

            var toAccount = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.AccountNumber == request.ToAccountNumber);

            if (fromAccount == null)
                return BadRequest("Sender account not found");

            if (toAccount == null)
                return BadRequest("Receiver account not found");

            if (fromAccount.Balance < request.Amount)
                return BadRequest("Insufficient balance");

            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                fromAccount.Balance -= request.Amount;
                toAccount.Balance += request.Amount;

                _context.Transactions.Add(new Transaction
                {
                    FromAccountId = fromAccount.Id,
                    ToAccountId = toAccount.Id,
                    Amount = request.Amount,
                    Type = "Transfer",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return Ok(new
                {
                    message = "Transfer successful ✅",
                    amount = request.Amount
                });
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                return StatusCode(500, "Transfer failed. Rolled back.");
            }
        }

        // 💰 DEPOSIT (SECURITY FIX)
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] AmountRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero");

            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.AccountNumber == request.AccountNumber &&
                    a.UserId == user.Id); // 🔥 SECURITY FIX

            if (account == null)
                return BadRequest("Account not found");

            account.Balance += request.Amount;

            _context.Transactions.Add(new Transaction
            {
                ToAccountId = account.Id,
                Amount = request.Amount,
                Type = "Deposit",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Deposit successful ✅",
                balance = account.Balance
            });
        }

        // 💸 WITHDRAW (SECURITY FIX)
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] AmountRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero");

            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.AccountNumber == request.AccountNumber &&
                    a.UserId == user.Id); // 🔥 SECURITY FIX

            if (account == null)
                return BadRequest("Account not found");

            if (account.Balance < request.Amount)
                return BadRequest("Insufficient balance");

            account.Balance -= request.Amount;

            _context.Transactions.Add(new Transaction
            {
                FromAccountId = account.Id,
                Amount = request.Amount,
                Type = "Withdraw",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Withdraw successful ✅",
                balance = account.Balance
            });
        }

        // 📜 HISTORY (IMPROVED)
        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var accounts = _context.Accounts
                .Where(a => a.UserId == userId)
                .ToList();

            var accountIds = accounts.Select(a => a.Id).ToList();

            // 🔥 IMPORTANT: AsEnumerable (avoid SQL issues)
            var transactions = _context.Transactions
                .AsEnumerable()
                .Where(t =>
                    accountIds.Contains(t.FromAccountId) ||
                    accountIds.Contains(t.ToAccountId)
                )
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            var result = transactions.Select(t => new
            {
                t.Id,
                t.Type,
                t.Amount,
                t.CreatedAt,
                FromAccount = accounts.FirstOrDefault(a => a.Id == t.FromAccountId)?.AccountNumber,
                ToAccount = accounts.FirstOrDefault(a => a.Id == t.ToAccountId)?.AccountNumber
            });

            return Ok(result);
        }
    }
}