using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using MusicLibraryNet.Services.Abstractions;

namespace MusicLibraryNet.Services;

public class EntityCrudExecutor<TIn, TOut> (ICrudService<TIn, TOut> service) : Controller
{
    public async Task<IActionResult> Get(Expression<Func<TOut, bool>>? predicate = null)
    {
        return await service.GetAsync(predicate) switch
        {
            Success<List<TOut>> success => Ok(success.Value),
            Fail<List<TOut>> fail => BadRequest(fail.Message),
            null => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<IActionResult> Get(TIn dto)
    {
        return await service.GetAsync(dto) switch
        {
            Success<TOut> success => Ok(success.Value),
            Fail<TOut> fail => BadRequest(fail.Message),
            null => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public async Task<IActionResult> Create(TIn dto)
    {
        return await service.CreateAsync(dto) switch
        {
            Success<TOut> success => Ok(success.Value),
            Fail<TOut> fail => BadRequest(fail.Message),
            null => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    
    public async Task<IActionResult> Update(TIn dto)
    {
        return await service.UpdateAsync(dto) switch
        {
            Success<TOut> success => Ok(success.Value),
            Fail<TOut> fail => BadRequest(fail.Message),
            null => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public async Task<IActionResult> Delete(TIn dto)
    {
        return await service.DeleteAsync(dto) switch
        {
            Success<TOut> success => Ok(success.Value),
            Fail<TOut> fail => BadRequest(fail.Message),
            null => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}