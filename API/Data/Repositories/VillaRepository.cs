using API.Data.Repositories.Interfaces;
using Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace API.Data.Repositories
{
    public class VillaRepository : Repository<Villa>, IVillaRepository
    {
        private readonly ApplicationDbContext _context;

        public VillaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Villa> UpdateAsync(Villa villa)
        {
            villa.UpdatedAt = DateTime.Now;

            _context.Villas.Update(villa);
            await _context.SaveChangesAsync();

            return villa;
        }
    }
}
