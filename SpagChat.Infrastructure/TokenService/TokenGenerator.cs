using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SpagChat.Application.Interfaces.IServices;
using SpagChat.Application.JWT;
using SpagChat.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SpagChat.Infrastructure.TokenService
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly JwtSetting _jwtSettings;
        private readonly ILogger<TokenGenerator> _logger;

        public TokenGenerator(IOptions<JwtSetting> jwtOptions, ILogger<TokenGenerator> logger)
        {
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }
        public string? GenerateAccessToken(ApplicationUser user)
        {
            if (user == null)
            {
                _logger.LogWarning("User object is null");
                return null;
            }

            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var key = symmetricSecurityKey;
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes).UtcDateTime,
                claims: claims,
                signingCredentials: creds);
            _logger.LogInformation($"This is your token :{token}");

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
