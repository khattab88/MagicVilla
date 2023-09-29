using Models;
using System.Linq.Expressions;

namespace API.Data.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, string includedProperties = null,
            int pageSize = 0, int pageNumber = 1);
        Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string includedProperties = null);
        Task CreateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SaveAsync();
    }
}
