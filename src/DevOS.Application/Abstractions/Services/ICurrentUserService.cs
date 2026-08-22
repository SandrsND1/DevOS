namespace DevOS.Application.Abstractions.Services
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
    }
}