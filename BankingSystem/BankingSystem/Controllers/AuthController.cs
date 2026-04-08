using BankingSystem.Application.DTOs;
using BankingSystem.Domain.Entities;
using BankingSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // 🔥 STEP 1: SEND OTP
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required");

            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            if (exists)
                return BadRequest("User already exists");

            var otpCode = new Random().Next(100000, 999999).ToString();

            var otp = new Otp
            {
                Email = email,
                Code = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5)
            };

            _context.Otps.Add(otp);
            await _context.SaveChangesAsync();

            Console.WriteLine($"OTP for {email}: {otpCode}");

            return Ok(new { message = "OTP sent successfully 📩" });
        }

        // 🔥 STEP 2: VERIFY OTP + REGISTER
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(
            [FromQuery] string otp,
            RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(otp))
                return BadRequest("OTP is required");

            var validOtp = await _context.Otps
                .Where(x => x.Email == request.Email && x.Code == otp)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (validOtp == null)
                return BadRequest("Invalid OTP");

            if (validOtp.ExpiryTime < DateTime.UtcNow)
                return BadRequest("OTP expired");

            var existingUser = await _context.Users
                .AnyAsync(u => u.Email == request.Email);

            if (existingUser)
                return BadRequest("User already exists");

            var user = new User
            {
                Name = request.Name ?? request.Email.Split('@')[0],
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User"
            };

            _context.Users.Add(user);
            _context.Otps.Remove(validOtp);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Account verified & created ✅" });
        }

        // 🔑 LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid email or password");

            var isValidPassword = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash
            );

            if (!isValidPassword)
                return Unauthorized("Invalid email or password");

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful ✅",
                token = token,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email
                }
            });
        }

        // 🎟️ GENERATE JWT TOKEN
        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ✅ VERY IMPORTANT
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}