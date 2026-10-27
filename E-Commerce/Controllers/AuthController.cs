using E_Commerce.Data;
using E_Commerce.DTOs;
using E_Commerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ECommerceDBcontext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<Customer> _passwordHasher;

        public AuthController(
            ECommerceDBcontext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<Customer>();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            var exists = await _context.Customers
                .AnyAsync(c => c.Email == email);

            if (exists)
            {
                return BadRequest(new
                {
                    message = "Email is already registered."
                });
            }

            var customer = new Customer
            {
                FullName = dto.FullName.Trim(),
                Email = email
            };

            customer.PasswordHash = _passwordHasher.HashPassword(
                customer,
                dto.Password
            );

            _context.Customers.Add(customer);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful.",
                customerId = customer.CustomerId,
                customer.FullName,
                customer.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);

            if (customer == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var result = _passwordHasher.VerifyHashedPassword(
                customer,
                customer.PasswordHash,
                dto.Password
            );

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is missing.");

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    customer.CustomerId.ToString()
                ),
                new Claim(ClaimTypes.Name, customer.FullName),
                new Claim(ClaimTypes.Email, customer.Email)
            };

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)
            );

            var credentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(
                        _configuration["Jwt:ExpiryMinutes"] ?? "30"
                    )
                ),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return Ok(new
            {
                message = "Login successful.",
                token = tokenString,
                customer = new
                {
                    customer.CustomerId,
                    customer.FullName,
                    customer.Email
                }
            });
        }
    }
}