using ChatApp.Api.Data;
using ChatApp.Api.DTOs;
using ChatApp.Api.DTOs.Auth;
using ChatApp.Api.Models;
using ChatApp.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ChatApp.Api.Helpers;

namespace ChatApp.Api.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenGenerator _jwtHelper;
        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _jwtHelper = new JwtTokenGenerator(config);
        }

        public async Task<Result<object>> Register(RegisterDto dto)
        {
            var isHasUser = await _context.Users.AnyAsync(u => u.Username == dto.Username);
            if (isHasUser)
                return Result<object>.Fail("Bu kullanıcı adı zaten alınmış. Lütfen başka bir tane deneyin.");

            _context.Users.Add(new User
            {
                Username = dto.Username,
                PasswordHash = PasswordHasher.HashPassword(dto.Password!)
            });

            await _context.SaveChangesAsync();
            return Result<object>.Ok();
        }

        public async Task<Result<LoginResponseDto>> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) return Result<LoginResponseDto>.Fail("Geçersiz kullanıcı adı.");


            var hash = PasswordHasher.HashPassword(dto.Password!);
            if (hash != user.PasswordHash) return Result<LoginResponseDto>.Fail("Geçersiz şifre.");

            var token = _jwtHelper.GenerateToken(user);
            var result = new LoginResponseDto { Token = token, UserId = user.Id, Username = user.Username };
            return Result<LoginResponseDto>.Ok(result);
        }
    }
}
