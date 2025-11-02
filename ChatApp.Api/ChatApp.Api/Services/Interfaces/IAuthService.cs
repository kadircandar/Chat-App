using ChatApp.Api.DTOs;
using ChatApp.Api.DTOs.Auth;

namespace ChatApp.Api.Services.Interfaces
{
    public interface IAuthService
    { 
        Task<Result<object>> Register(RegisterDto dto);
        Task<Result<LoginResponseDto>> Login(LoginDto dto);
    }
}
