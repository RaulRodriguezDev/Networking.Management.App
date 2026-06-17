using System.Linq.Expressions;

namespace Network.Management.App.Repository
{
    public interface IDbRepository
    {
        Task<IEnumerable<T>> GetAllAsync<T>() where T : class;

        Task<IEnumerable<TDto>> GetAllAsync<T, TDto>(Expression<Func<T, TDto>> selector) where T : class;

        Task<IEnumerable<TDto>> GetWhereAsync<T, TDto>(Expression<Func<T, bool>> predicate, Expression<Func<T, TDto>> selector) where T : class;

        Task<T?> GetByIdAsync<T>(object id) where T : class;

        Task AddAsync<T>(T entity) where T : class;

        Task UpdateAsync<T>(T entity) where T : class;

        Task DeleteAsync<T>(object id) where T : class;
    }
}
