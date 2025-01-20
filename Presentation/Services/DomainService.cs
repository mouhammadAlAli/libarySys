using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositries;
using Domain.Repositries.Common;
using Domain.Services;
using System.Linq.Expressions;

namespace Presentation.Services;

internal class DomainService<TEntity, TDto, TCreate> : IDomainService<TEntity, TDto, TCreate> where TEntity : BaseEntity
{
    protected readonly IRepository<TEntity> _repository;
    protected readonly IMapper _mapper;
    public DomainService(IRepository<TEntity> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    public virtual Task CreateAsync(TCreate create)
    {
        var entity = _mapper.Map<TEntity>(create);
        return _repository.CreateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.FirstOrDefaultAsync(c => c.Id == id);
        if (entity == null)
        {
            throw new NotFoundException(nameof(TEntity), id.ToString());
        }
        await _repository.DeleteAsync(entity);
    }

    public async Task<TDto> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        var entity = await _repository.FirstOrDefaultAsync(filter, propertySelectors);
        return _mapper.Map<TDto>(entity);
    }

    public async Task<IEnumerable<TDto>> GetAll(Expression<Func<TEntity, bool>> filter = null, Dictionary<string, object> searchCriteria = null, params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        var data = await _repository.GetAllAsync(filter, searchCriteria, propertySelectors);
        return _mapper.Map<IEnumerable<TDto>>(data);
    }

    public async Task<PageResult<TDto>> GetPageAsync(PageRequest pageRequest, Expression<Func<TEntity, bool>> filter, Dictionary<string, object> searchCriteria = null, params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        var result = await _repository.GetPageAsync(pageRequest, filter, searchCriteria, propertySelectors);
        return new PageResult<TDto>()
        {
            TotalRecords = result.TotalRecords,
            Data = _mapper.Map<IEnumerable<TDto>>(result.Data)
        };
    }
    public async Task UpdateAsync(int id, TCreate create)
    {
        var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            throw new NotFoundException(nameof(TEntity), id.ToString());
        }

        _mapper.Map(create, entity);
        await _repository.UpdateAsync(entity);
    }

}
