using Domain.Entities;
using Domain.Repositries.Common;
using System.Linq.Expressions;

namespace Domain.Repositries;

public interface IRepository<T> where T : BaseEntity
{
    Task CreateAsync(T entity);
    Task CreateRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<PageResult<T>> GetPageAsync(PageRequest pageRequest, Expression<Func<T, bool>> filter, Dictionary<string, object> searchCriteria = null, params Expression<Func<T, object>>[] propertySelectors);
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, Dictionary<string, object> searchCriteria = null, params Expression<Func<T, object>>[] propertySelectors);
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] propertySelectors);
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
}
