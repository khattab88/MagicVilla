using Models.Dtos.Identity;

namespace Web.Services.Interfaces
{
    public interface IAuthService
    {
        Task<T> LoginAsync<T>(LoginRequestDto loginRequest);
        Task<T> RegisterAsync<T>(RegisterRequestDto registerRequest);
        Task<T> LogoutAsync<T>(TokenDto token);
    }
}
