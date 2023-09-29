using Models.Dtos.Identity;
using Utilities;
using Web.Services.Interfaces;

namespace Web.Services
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public TokenProvider(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public void Clear()
        {
            _contextAccessor.HttpContext?.Response.Cookies.Delete(Constants.AccessToken);
            _contextAccessor.HttpContext?.Response.Cookies.Delete(Constants.RefreshToken);
        }

        public TokenDto Get()
        {
            try
            {
                bool hasAccessToken = _contextAccessor.HttpContext.Request.Cookies.TryGetValue(Constants.AccessToken, out string accessToken);
                bool hasRefreshToken = _contextAccessor.HttpContext.Request.Cookies.TryGetValue(Constants.RefreshToken, out string refreshToken);

                TokenDto tokenDto = new TokenDto()
                {
                    AccessToken = accessToken,
                    RefreshToken= refreshToken
                };

                return hasAccessToken? tokenDto : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Set(TokenDto token)
        {
            var cookieOptions = new CookieOptions() { Expires = DateTime.Now.AddDays(60) };
            _contextAccessor.HttpContext?.Response.Cookies.Append(Constants.AccessToken, token.AccessToken, cookieOptions);
            _contextAccessor.HttpContext?.Response.Cookies.Append(Constants.RefreshToken, token.AccessToken, cookieOptions);
        }
    }
}
