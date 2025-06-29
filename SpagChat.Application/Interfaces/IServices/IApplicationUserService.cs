using SpagChat.Application.DTO.Auth;
using SpagChat.Application.DTO.LoginResponse;
using SpagChat.Application.DTO.Users;
using SpagChat.Application.Result;

namespace SpagChat.Application.Interfaces.IServices
{
    public interface IApplicationUserService
    {
        Task<Result<LoginResponseDto>> LoginAsync(LoginUserDto userDetails);
        Task<Result<bool>> DeleteUsersAsync(List<Guid> userIds, bool useParallel = false);
        Task<Result<Guid>> RegisterUserAsync(CreateUserDto userDetails);
        Task<Result<ApplicationUserDto>> GetUserByIdAsync(Guid userId);
        Task<Result<IEnumerable<ApplicationUserDto>?>> GetAllUsersAsync(int numberOfUsers);
    }
}
