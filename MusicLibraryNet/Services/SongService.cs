using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicLibraryNet.Database;
using MusicLibraryNet.Database.Auth;
using MusicLibraryNet.Database.Music;
using MusicLibraryNet.Dto.Music;
using MusicLibraryNet.Services.Abstractions;

namespace MusicLibraryNet.Services;

public class SongService (
    ApplicationDbContext context,
    UserManager<MusicUser> userManager,
    IHttpContextAccessor httpContextAccessor
) : ISongService<SongDto, Song>
{
    public async Task<OperationResult<Song>> GetAsync(SongDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Song>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Song>("User not found");
        
        var song  = await context.Songs.FirstOrDefaultAsync(s => s.Id == dto.Id);
        if (song is null) return new Fail<Song>($"Song with ID: {dto.Id} not found");
        
        return new Success<Song>(song);
        
    }

    public async Task<OperationResult<List<Song>>> GetAsync()
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<List<Song>>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<List<Song>>("User not found");
        
        var song = await context.Songs.Where(s => s.OwnerId == user.Id).ToListAsync();
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
        };
        
        await context.Songs.AddAsync(song);

        await context.SaveChangesAsync();
        
        return new Success<Song>(song);
    }

    public async Task<OperationResult<Song>> UpdateAsync(SongDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Song>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Song>("User not found");

        var existedSong = await context.Songs.FirstOrDefaultAsync(s => s.Id == dto.Id);
        if (existedSong is null) return new Fail<Song>($"Song with ID: {dto.Id} not found");

        existedSong.Title = dto.Title;
        existedSong.Artist = dto.Artist;
        existedSong.Description = dto.Description;
        
        context.Songs.Update(existedSong);
        
        await context.SaveChangesAsync();
        
        return new Success<Song>(existedSong);
        
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
