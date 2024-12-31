using System.Linq.Expressions;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicLibraryNet.Database;
using MusicLibraryNet.Database.Auth;
using MusicLibraryNet.Database.Music;
using MusicLibraryNet.Dto.Music;
using MusicLibraryNet.Providers;
using MusicLibraryNet.Services.Abstractions;
using MusicLibraryNet.Utils;
using File = MusicLibraryNet.Database.File;

namespace MusicLibraryNet.Services;

public class SongService (
    ApplicationDbContext context,
    UserManager<MusicUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    MusicStoreProvider musicStoreProvider
) : ISongService<SongDto, Song>
{
    public async Task<OperationResult<Song>> GetAsync(SongDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Song>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Song>("User not found");

        var songQuery = context.Songs.Where(s => s.OwnerId == user.Id);
        if (dto.Id > 0) songQuery = songQuery.Where(s => s.Id == dto.Id);
        if (!string.IsNullOrEmpty(dto.Title))
        {
            songQuery = songQuery.Where(s => EF.Functions.ILike(s.Title, $"%{dto.Title}%"));
        }
        
        var song = await songQuery.FirstOrDefaultAsync();
        if (song is null) return new Fail<Song>($"Song with ID: {dto.Id} not found");
        
        return new Success<Song>(new Song
        {
            Id = song.Id,
            Description = song.Description,
            Artist = song.Artist,
            Title = song.Title,
            OwnerId = user.Id
        });
        
    }

    public async Task<OperationResult<List<Song>>> GetAsync(Expression<Func<Song, bool>>? filter = null)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<List<Song>>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<List<Song>>("User not found");
        
        var song = await context.Songs
            .Where(s => s.OwnerId == user.Id)
            .Where(filter ?? (_ => true))
            .ToListAsync();
        return new Success<List<Song>>(song);
    }

    public async Task<OperationResult<Song>> CreateAsync(SongDto dto)
    {
        
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Song>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Song>("User not found");

        var existedSong = await context.Songs.FirstOrDefaultAsync(s => s.Title == dto.Title && s.OwnerId == user.Id);
        if (existedSong is not null) return new Fail<Song>("Song with ");

        var song = new Song
        {
            Title = dto.Title,
            OwnerId = user!.Id,
            Artist = dto.Artist,
            Description = dto.Description,
            Files = []
        };
        
        var files = dto.Files ?? [];
        if (files.Count > 0)
        {
            musicStoreProvider.CreateDirectory($"{user.Id}");
        }
        
        foreach (var formFile in dto.Files ?? [])
        {
            var bytes = await FileUtils.ConvertFormFileToBytes(formFile);
            var path = Path.Combine($"{user.Id}", formFile.FileName);
            await musicStoreProvider.CreateFile(bytes, path);
            song.Files.Add(new File
            {
                Path = path
            });
        }
        
        
        await context.Songs.AddAsync(song);

        await context.SaveChangesAsync();
        
        return new Success<Song>(new Song
        {
            Id = song.Id
        });
    }

    public async Task<OperationResult<Song>> UpdateAsync(SongDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Song>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Song>("User not found");

        var existedSong = await context.Songs
            .Include(s => s.Files)
            .FirstOrDefaultAsync(s => s.Id == dto.Id);
        if (existedSong is null) return new Fail<Song>($"Song with ID: {dto.Id} not found");

        existedSong.Title = dto.Title;
        existedSong.Artist = dto.Artist;
        existedSong.Description = dto.Description;
        
        var files = dto.Files ?? [];
        if (files.Count > 0)
        {
            musicStoreProvider.CreateDirectory($"{user.Id}");
        }
        
        foreach (var formFile in files)
        {
            var bytes = await FileUtils.ConvertFormFileToBytes(formFile);
            var path = Path.Combine($"{user.Id}", formFile.FileName);
            await musicStoreProvider.CreateFile(bytes, path);
            existedSong.Files.Add(new File
            {
                Path = path
            });
        }
        
        
        
        context.Songs.Update(existedSong);
        
        await context.SaveChangesAsync();
        
        return new Success<Song>(new Song{ Id = existedSong.Id, Title = existedSong.Title, Artist = existedSong.Artist, Description = existedSong.Description });
        
    }

    public async Task<OperationResult<Song>> DeleteAsync(SongDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null) return new Fail<Song>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Song>("User not found");

        var existedSong = await context.Songs.FirstOrDefaultAsync(s => s.Id == dto.Id);
        if (existedSong is null) return new Fail<Song>($"Song with ID: {dto.Id} not found");
        
        context.Songs.Remove(existedSong);
        await context.SaveChangesAsync();
        return new Success<Song>(existedSong);
    }
}
