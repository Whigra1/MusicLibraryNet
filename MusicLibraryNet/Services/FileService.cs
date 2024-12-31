using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicLibraryNet.Database;
using MusicLibraryNet.Database.Auth;
using MusicLibraryNet.Dto;
using MusicLibraryNet.Providers;
using MusicLibraryNet.Services.Abstractions;
using MusicLibraryNet.Utils;
using File = MusicLibraryNet.Database.File;

namespace MusicLibraryNet.Services;

public class FileService (
    ApplicationDbContext context, 
    UserManager<MusicUser> userManager, 
    IHttpContextAccessor httpContextAccessor,
    MusicStoreProvider musicStoreProvider
): IFileService<FileDto, File>
{
    public async Task<OperationResult<File>> GetAsync(FileDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<File>("User not found");

        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<File>("User not found");
        
        var file = await context.Files
            .Include(f => f.Song)
            .Where(f => f.Id == dto.Id && f.Song.OwnerId == user.Id)
            .FirstOrDefaultAsync();
        if (file is null) return new Fail<File>($"Song with ID: {dto.Id} not found or not accessible");
        return new Success<File>(file);
    }

    public async Task<OperationResult<List<File>>> GetAsync(Expression<Func<File, bool>>? filter = null)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<List<File>>("User not found");

        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<List<File>>("User not found");
        
        var files = await context.Files
            .Where(f => f.Song.OwnerId == user.Id)
            .Where(filter ?? (_ => true))
            .ToListAsync();
        
        foreach (var file in files)
        {
            file.Song = null!;
        }
        
        return new Success<List<File>>(files);
    }

    public async Task<OperationResult<File>> CreateAsync(FileDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<File>("User not found");

        if (dto.FileData is null) return new Fail<File>("File data not provided");
        if (dto.SongId is null) return new Fail<File>("Song ID not provided");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<File>("User not found");
       
        
        musicStoreProvider.CreateDirectory($"{user.Id}");
        var path = Path.Combine($"{user.Id}", dto.FileData.FileName);
        var result = await musicStoreProvider.CreateFile(await FileUtils.ConvertFormFileToBytes(dto.FileData), path);
        if (!result) return new Fail<File>("File creation failed");
        var file = new File
        {
            Path = path,
            SongId = (int) dto.SongId
        };
        
        await context.Files.AddAsync(file);
        await context.SaveChangesAsync();
        return new Success<File>(file);
    }

    public Task<OperationResult<File>> UpdateAsync(FileDto dto)
    {
        return Task.FromResult<OperationResult<File>>(new Fail<File>("Not implemented"));
    }

    public async Task<OperationResult<File>> DeleteAsync(FileDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<File>("User not found");

        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<File>("User not found");

        var file = await context.Files
            .Include(f => f.Song)
            .Where(f => f.Id == dto.Id && f.Song.OwnerId == user.Id)
            .FirstOrDefaultAsync();
        if (file is null) return new Fail<File>($"Song with ID: {dto.Id} not found or not accessible");

        
        if (!musicStoreProvider.RemoveFile(file.Path))
            return new Fail<File>("File deletion failed");
        context.Files.Remove(file);
        await context.SaveChangesAsync();
        return new Success<File>(file);
        
    }
}