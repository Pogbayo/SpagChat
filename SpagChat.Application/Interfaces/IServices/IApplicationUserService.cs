using SpagChat.Application.DTO.Auth;
using SpagChat.Application.DTO.Users;
using SpagChat.Application.Result;

namespace SpagChat.Application.Interfaces.IServices
{
    public interface IApplicationUserService
    {
        Task<Result<string>> LoginAsync(LoginUserDto userDetails);
        Task<Result<Guid>> RegisterUserAsync(CreateUserDto userDetails);
        Task<Result<ApplicationUserDto>> GetUserByIdAsync(Guid userId);
        Task<Result<IEnumerable<ApplicationUserDto>?>> GetAllUsersAsync(int numberOfUsers);
    }
}
