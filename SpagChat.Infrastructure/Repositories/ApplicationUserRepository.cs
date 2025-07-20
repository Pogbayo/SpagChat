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

        public async Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser?> FindByUsernameAsync(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        public async Task<IdentityResult> UpdateUsernameAsync(Guid userId, string newUsername)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            user.UserName = newUsername;
            user.NormalizedUserName = newUsername.ToUpper();
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> UpdatePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<bool> DeleteUsersAsync(List<Guid> userIds, bool useParallel = false)
        {
            var usersToDelete = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            if (!usersToDelete.Any())
                return false;

            if (useParallel)
            {
                var deleteTasks = usersToDelete.Select(user => _userManager.DeleteAsync(user));
                await Task.WhenAll(deleteTasks);
            }
            else
            {
                foreach (var user in usersToDelete)
                {
                    await _userManager.DeleteAsync(user);
                }
            }

            return true;
        }


    }
}
