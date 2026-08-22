using DevOS.Domain.Entities;

namespace DevOS.Application.Abstractions.Services
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}