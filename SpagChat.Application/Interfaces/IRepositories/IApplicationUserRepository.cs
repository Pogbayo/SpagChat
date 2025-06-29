using Microsoft.AspNetCore.Identity;
using SpagChat.Application.DTO.Auth;
using SpagChat.Domain.Entities;


namespace SpagChat.Application.Interfaces.IRepositories
{
    public interface IApplicationUserRepository
    {
        Task<ApplicationUser?> LoginAsync(LoginUserDto userDetails);
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<ApplicationUser?> FindByUsernameAsync(string username);
        Task<IdentityResult?> CreateUserAsync(ApplicationUser user, string password);
        Task<ApplicationUser?> GetUserById(Guid userId);
        Task<bool> DeleteUsersAsync(List<Guid> userIds,bool useParallel = false);
        Task<IEnumerable<ApplicationUser>?> GetAllUsers(int numberOfUsers);
    }
}
