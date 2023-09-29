using API.Data.Repositories.Interfaces;
using Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography.Xml;

namespace API.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            this.dbSet = _context.Set<T>();
        }

        public async Task CreateAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            await SaveAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            dbSet.Remove(entity);
            await SaveAsync();
        }

        public Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, string includedProperties = null,
            int pageSize = 0, int pageNumber = 1)
        {
            IQueryable<T> queryable = dbSet;

            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }

            if (!string.IsNullOrEmpty(includedProperties)) 
            {
                foreach (var prop in includedProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
					queryable = queryable.Include(includedProperties);
				}
            }

            if(pageSize > 0)
            {
                return queryable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            }
            else
            {
                return queryable.ToListAsync();
            }
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string includedProperties = null)
        {
            IQueryable<T> queryable = dbSet;

            if (!tracked)
            {
                queryable = queryable.AsNoTracking();
            }

            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }

			if (!string.IsNullOrEmpty(includedProperties))
			{
				foreach (var prop in includedProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					queryable = queryable.Include(includedProperties);
				}
			}

			return await queryable.FirstOrDefaultAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
