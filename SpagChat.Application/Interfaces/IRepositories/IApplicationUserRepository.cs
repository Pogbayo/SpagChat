using Microsoft.AspNetCore.Identity;
using SpagChat.Application.DTO.Auth;
using SpagChat.Domain.Entities;


namespace SpagChat.Application.Interfaces.IRepositories
{
    public interface IApplicationUserRepository
    {
        Task<ApplicationUser?> LoginAsync(LoginUserDto userDetails);
        Task<IdentityResult?> CreateUserAsync(ApplicationUser user, string password);
        Task<ApplicationUser?> GetUserById(Guid userId);
        Task<IEnumerable<ApplicationUser>?> GetAllUsers(int numberOfUsers);
    }
}
