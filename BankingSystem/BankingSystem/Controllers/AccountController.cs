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
    [Authorize] // 🔐 All endpoints require authentication
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // 🔑 Helper: Get Logged-in UserId
        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            return int.Parse(userIdClaim);
        }

        // 💳 Create Account
        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount()
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized("Invalid token");

            // 🔍 Check if user already has account (optional rule)
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (existingAccount != null)
            {
                return BadRequest("User already has an account");
            }

            var account = new Account
            {
                UserId = userId.Value,
                AccountNumber = GenerateAccountNumber(),
                Balance = 0,
                AccountType = "Savings",
             
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Account created successfully ✅",
                account = new
                {
                    account.Id,
                    account.AccountNumber,
                    account.Balance,
                    account.AccountType
                }
            });
        }

        // 📄 Get All Accounts of Logged-in User
        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized("Invalid token");

            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId.Value)
                .ToListAsync();

            return Ok(accounts);
        }

        // ⭐ Get Primary Account (BEST PRACTICE)
        [HttpGet("my-account")]
        public async Task<IActionResult> GetMyAccount()
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized("Invalid token");

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId.Value);

            if (account == null)
                return NotFound("Account not found");

            return Ok(new
            {
                account.Id,
                account.AccountNumber,
                account.Balance,
                account.AccountType
            });
        }

        // 💰 Deposit Money (Optional but useful)
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] decimal amount)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized("Invalid token");

            if (amount <= 0)
                return BadRequest("Invalid amount");

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId.Value);

            if (account == null)
                return NotFound("Account not found");

            account.Balance += amount;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Deposit successful ✅",
                balance = account.Balance
            });
        }

        // 🔢 Generate Account Number
        private string GenerateAccountNumber()
        {
            return "ACC" + DateTime.UtcNow.Ticks.ToString().Substring(5);
        }
    }
}