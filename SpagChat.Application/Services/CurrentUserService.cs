using Microsoft.AspNetCore.Http;
using SpagChat.Application.Interfaces.IServices;

namespace SpagChat.Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("id"); 
            return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : (Guid?)null;
        }
    }
}
