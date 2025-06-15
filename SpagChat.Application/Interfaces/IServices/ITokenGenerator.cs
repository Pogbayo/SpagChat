using SpagChat.Domain.Entities;

namespace SpagChat.Application.Interfaces.IServices
{
    public interface ITokenGenerator
    {
        string? GenerateAccessToken(ApplicationUser user);
    }
}
