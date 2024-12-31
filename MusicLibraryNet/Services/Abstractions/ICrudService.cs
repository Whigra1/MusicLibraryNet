using System.Linq.Expressions;

namespace MusicLibraryNet.Services.Abstractions;

public interface ICrudService<TDto, TResult>    
{
    public Task<OperationResult<TResult>> GetAsync(TDto dto);
    public Task<OperationResult<List<TResult>>> GetAsync(Expression<Func<TResult, bool>>? queryBuilder = null);
    public Task<OperationResult<TResult>> CreateAsync(TDto dto);
    public Task<OperationResult<TResult>> UpdateAsync(TDto dto);
    public Task<OperationResult<TResult>> DeleteAsync(TDto dto);
}