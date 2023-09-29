using Microsoft.AspNetCore.Http.HttpResults;
using Models;
using Models.Dtos.Identity;
using NuGet.Protocol.Plugins;
using Utilities.Enums;
using Web.Services.Interfaces;

namespace Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IBaseService _baseSvc;

        private string _url;

        public AuthService(
            IHttpClientFactory httpClient, 
            IBaseService baseSvc,
            IConfiguration configuration) 
        {
            _httpClient = httpClient;
            _baseSvc = baseSvc;
            _url = configuration.GetValue<string>("Services:VillaAPI");
        }

        public async Task<T> LoginAsync<T>(LoginRequestDto loginRequest)
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest
            {
                RequestType = RequestType.POST,
                Url = $"{_url}/api/UsersAuth/login",
                Data = loginRequest
            });
        }

        public async Task<T> RegisterAsync<T>(RegisterRequestDto registerRequest)
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest
            {
                RequestType = RequestType.POST,
                Url = $"{_url}/api/UsersAuth/register",
                Data = registerRequest
            });
        }

        public async Task<T> LogoutAsync<T>(TokenDto token)
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest
            {
                RequestType = RequestType.POST,
                Url = $"{_url}/api/UsersAuth/revoke",
                Data = token
            });
        }
    }
}
