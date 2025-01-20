using Domain.Entities;
using Domain.Repositries.Common;
using System.Linq.Expressions;

namespace Domain.Services;

public interface IDomainService<TEntity, TDto, TCreate> where TEntity : BaseEntity
{
    Task<PageResult<TDto>> GetPageAsync(PageRequest pageRequest, Expression<Func<TEntity, bool>> filter, Dictionary<string, object> searchCriteria = null, params Expression<Func<TEntity, object>>[] propertySelectors);
    Task<IEnumerable<TDto>> GetAll(Expression<Func<TEntity, bool>> filter = null, Dictionary<string, object> searchCriteria = null, params Expression<Func<TEntity, object>>[] propertySelectors);
    Task CreateAsync(TCreate create);
    Task<TDto> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] propertySelectors);
    Task UpdateAsync(int id, TCreate create);
    Task DeleteAsync(int id);
}
