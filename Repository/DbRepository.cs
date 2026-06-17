using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Network.Management.App.Repository
{
    public class DbRepository : IDbRepository
    {
        private readonly AppDbContext _context;

        public DbRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<TDto>> GetAllAsync<T, TDto>(Expression<Func<T, TDto>> selector) where T : class
        {
            return await _context.Set<T>().Select(selector).ToListAsync();
        }

        public async Task<IEnumerable<TDto>> GetWhereAsync<T, TDto>(Expression<Func<T, bool>> predicate, Expression<Func<T, TDto>> selector) where T : class
        {
            return await _context.Set<T>().Where(predicate).Select(selector).ToListAsync();
        }

        public async Task<T?> GetByIdAsync<T>(object id) where T : class
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task AddAsync<T>(T entity) where T : class
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync<T>(T entity) where T : class
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync<T>(object id) where T : class
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity is not null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
