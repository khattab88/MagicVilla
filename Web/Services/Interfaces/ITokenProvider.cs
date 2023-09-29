using Models.Dtos.Identity;

namespace Web.Services.Interfaces
{
    public interface ITokenProvider
    {
        void Set(TokenDto token);
        TokenDto? Get();
        void Clear();
    }
}
