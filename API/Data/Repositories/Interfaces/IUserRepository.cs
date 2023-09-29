using Models;
using Models.Dtos.Identity;

namespace API.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserDto> Register(RegisterRequestDto registerRequest);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequest);
        bool IsUnique(string userName);
        Task<TokenDto> RefreshAccessToken(TokenDto token);

        Task RevokeRefreshToken(TokenDto token);
    }
}
