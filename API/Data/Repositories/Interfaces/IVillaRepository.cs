using Models;
using System.Linq.Expressions;

namespace API.Data.Repositories.Interfaces
{
    public interface IVillaRepository : IRepository<Villa>
    {
        Task<Villa> UpdateAsync(Villa villa);        
    }
}
