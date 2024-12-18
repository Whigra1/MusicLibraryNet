using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MusicLibraryNet.Services.Abstractions;

namespace MusicLibraryNet.Services;

public class EntityCrudExecutor<TIn, TOut> (ICrudService<TIn, TOut> service) : Controller
{
    public async Task<IActionResult> Get()
    {
        return await service.GetAsync() switch
        {
            Fail<List<TOut>> fail => BadRequest(fail.Message),
            Success<List<TOut>> success => Ok(success.Value),
            null => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<IActionResult> Get(TIn dto)
    {
        return await service.GetAsync(dto) switch
        {
            Fail<List<TOut>> fail => BadRequest(fail.Message),
            Success<List<TOut>> success => Ok(success.Value),
            null => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public async Task<IActionResult> Create(TIn dto)
    {
        return await service.CreateAsync(dto) switch
        {
            Fail<List<TOut>> fail => BadRequest(fail.Message),
            Success<List<TOut>> success => Ok(success.Value),
            null => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    
    public async Task<IActionResult> Update(TIn dto)
    {
        return await service.UpdateAsync(dto) switch
        {
            Fail<List<TOut>> fail => BadRequest(fail.Message),
            Success<List<TOut>> success => Ok(success.Value),
            null => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public async Task<IActionResult> Delete(TIn dto)
    {
        return await service.DeleteAsync(dto) switch
        {
            Fail<List<TOut>> fail => BadRequest(fail.Message),
            Success<List<TOut>> success => Ok(success.Value),
            null => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}