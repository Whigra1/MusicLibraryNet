using MusicLibraryNet.Dto;
namespace MusicLibraryNet.Services.Abstractions;
using Database;

public interface IFileService<TIn, TOut> : ICrudService<TIn, TOut>
{
    
}