using Models.Dtos;

namespace Web.Services.Interfaces
{
    public interface IVillaService
    {
        Task<T> GetAllAsync<T>();
        Task<T> GetAsync<T>(int id);
        Task<T> CreateAsync<T>(VillaCreateDto createDto);
        Task<T> UpdateAsync<T>(VillaUpdateDto updateDto);
        Task<T> DeleteAsync<T>(int id);
    }
}
