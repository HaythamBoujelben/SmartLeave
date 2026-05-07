using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartLeave.Application.DTOs.Auth;
using SmartLeave.Application.Interfaces;
using SmartLeave.Domain.Entities;
using SmartLeave.Infrastructure.Persistence;

namespace SmartLeave.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Check if email already exists
            var exists = await _context.Employees
                .AnyAsync(e => e.Email == dto.Email);

            if (exists)
                throw new Exception("Email already registered.");

            var employee = new Employee
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                DepartmentId = dto.DepartmentId
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return GenerateToken(employee);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == dto.Email);

            if (employee == null || !BCrypt.Net.BCrypt.Verify(dto.Password, employee.PasswordHash))
                throw new Exception("Invalid email or password.");

            return GenerateToken(employee);
        }

        private AuthResponseDto GenerateToken(Employee employee)
        {
            var secret = _config["JwtSettings:Secret"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(
                double.Parse(_config["JwtSettings:ExpiryMinutes"]!));

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new Claim(ClaimTypes.Email, employee.Email),
            new Claim(ClaimTypes.Name, employee.FullName),
            new Claim(ClaimTypes.Role, employee.Role)
        };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                FullName = employee.FullName,
                Email = employee.Email,
                Role = employee.Role,
                ExpiresAt = expiry
            };
        }
    }
}
