using Models;

namespace Web.Services.Interfaces
{
    public interface IBaseService
    {
        ApiResponse Response { get; set; }
        Task<T> SendRequestAsync<T>(ApiRequest request);
    }
}
