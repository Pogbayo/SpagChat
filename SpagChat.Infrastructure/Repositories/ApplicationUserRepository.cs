using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpagChat.Application.DTO.Auth;
using SpagChat.Application.Interfaces.IRepositories;
using SpagChat.Domain.Entities;

namespace SpagChat.Infrastructure.Repositories
{
    public class ApplicationUserRepository : IApplicationUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationUserRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<ApplicationUser>?> GetAllUsers(int numberOfUsers)
        {
            return await _userManager.Users
                   .Take(numberOfUsers)
                   .ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserById(Guid userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString());
        }

        public async Task<ApplicationUser?> LoginAsync(LoginUserDto userDetails)
        {
            var user = await _userManager.FindByEmailAsync(userDetails.Email);
            if (user == null)
                return null;

            var passwordValid = await _userManager.CheckPasswordAsync(user, userDetails.Password);
            if (!passwordValid)
                return null;

            return user;
        }

        public async Task<IdentityResult?> CreateUserAsync(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }
    }
}
