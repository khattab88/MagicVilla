using Models;
using System.Linq.Expressions;

namespace API.Data.Repositories.Interfaces
{
    public interface IVillaNumberRepository : IRepository<VillaNumber>
    {
        Task<VillaNumber> UpdateAsync(VillaNumber villaNumber);        
    }
}
