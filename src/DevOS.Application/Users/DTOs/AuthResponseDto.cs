namespace DevOS.Application.Users.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; init; } = string.Empty;
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
    }
}