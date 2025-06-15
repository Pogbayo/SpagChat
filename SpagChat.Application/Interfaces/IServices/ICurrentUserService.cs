

namespace SpagChat.Application.Interfaces.IServices
{
    public interface ICurrentUserService
    {
        public Guid? GetCurrentUserId();
    }
}
