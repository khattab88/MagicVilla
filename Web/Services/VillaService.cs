using Microsoft.AspNetCore.Http.HttpResults;
using Models;
using Models.Dtos;
using Models.Enums;
using Utilities.Enums;
using Web.Services.Interfaces;

namespace Web.Services
{
    public class VillaService : IVillaService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IBaseService _baseSvc;

        private string _url;

        public VillaService(
            IHttpClientFactory httpClient,
            IBaseService baseSvc,
            IConfiguration configuration) 
        {
            _httpClient = httpClient;
            _baseSvc = baseSvc;
            _url = configuration.GetValue<string>("Services:VillaAPI");
        }

        public async Task<T> CreateAsync<T>(VillaCreateDto createDto)
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest 
            {
                RequestType = RequestType.POST,
                Url = $"{_url}/api/villas",
                Data = createDto,
                ContentType = ContentType.MultiPartFormData
            });
        }

        public async Task<T> DeleteAsync<T>(int id)
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest
            {
                RequestType = RequestType.DELETE,
                Url = $"{_url}/api/villas/{id}",
            });
        }

        public async Task<T> GetAllAsync<T>()
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest
            {
                RequestType = RequestType.GET,
                Url = $"{_url}/api/villas",
            });
        }

        public async Task<T> GetAsync<T>(int id)
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest
            {
                RequestType = RequestType.GET,
                Url = $"{_url}/api/villas/{id}",
            });
        }

        public async Task<T> UpdateAsync<T>(VillaUpdateDto updateDto)
        {
            return await _baseSvc.SendRequestAsync<T>(new ApiRequest
            {
                RequestType = RequestType.PUT,
                Url = $"{_url}/api/villas/{updateDto.Id}",
                Data = updateDto,
                ContentType = ContentType.MultiPartFormData
            });
        }
    }
}
