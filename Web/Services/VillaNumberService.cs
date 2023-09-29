using Microsoft.AspNetCore.Http.HttpResults;
using Models;
using Models.Dtos;
using Utilities.Enums;
using Web.Services.Interfaces;

namespace Web.Services
{
    public class VillaNumberService : IVillaNumberService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IBaseService _baseSvc;

        private string _url;

        public VillaNumberService(
            IHttpClientFactory httpClient,
            IBaseService baseSvc,
            IConfiguration configuration) 
        {
            _httpClient = httpClient;
            _baseSvc = baseSvc;
            _url = configuration.GetValue<string>("Services:VillaAPI");
        }

        public async Task<T> CreateAsync<T>(VillaNumberCreateDto createDto)
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest 
            {
                RequestType = RequestType.POST,
                Url = $"{_url}/api/villanumbers",
                Data = createDto,
            });
        }

        public async Task<T> DeleteAsync<T>(int number)
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest
            {
                RequestType = RequestType.DELETE,
                Url = $"{_url}/api/villavillanumbers/{number}",
            });
        }

        public async Task<T> GetAllAsync<T>()
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest
            {
                RequestType = RequestType.GET,
                Url = $"{_url}/api/villanumbers",
            });
        }

        public async Task<T> GetAsync<T>(int number)
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest
            {
                RequestType = RequestType.GET,
                Url = $"{_url}/api/villanumbers/{number}",
            });
        }

        public async Task<T> UpdateAsync<T>(VillaNumberUpdateDto updateDto)
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest
            {
                RequestType = RequestType.PUT,
                Url = $"{_url}/api/villanumbers/{updateDto.Number}",
                Data = updateDto,
            });
        }
    }
}
