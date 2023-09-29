using API.Data.Repositories.Interfaces;
using Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace API.Data.Repositories
{
    public class VillaNumberRepository : Repository<VillaNumber>, IVillaNumberRepository
    {
        private readonly ApplicationDbContext _context;

        public VillaNumberRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<VillaNumber> UpdateAsync(VillaNumber villaNumber)
        {
            villaNumber.UpdatedAt = DateTime.Now;

            _context.VillaNumbers.Update(villaNumber);
            await _context.SaveChangesAsync();

            return villaNumber;
        }
    }
}
