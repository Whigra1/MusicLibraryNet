using MusicLibraryNet.Database;
using MusicLibraryNet.Database.Auth;
using MusicLibraryNet.Database.Music;
using MusicLibraryNet.Dto.Music;
using MusicLibraryNet.Services.Abstractions;

namespace MusicLibraryNet.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


public class PlaylistService(
    ApplicationDbContext context, 
    UserManager<MusicUser> userManager, 
    IHttpContextAccessor httpContextAccessor
) : IPlaylistService<PlaylistDto, Playlist>
{
    
    public async Task<OperationResult<List<Playlist>>> GetAsync()
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<List<Playlist>>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<List<Playlist>>("User not found");
        
        var song = await context.Playlists.Where(s => s.OwnerId == user.Id).ToListAsync();
        return new Success<List<Playlist>>(song);
    }
    
    public async Task<OperationResult<Playlist>> GetAsync(PlaylistDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Playlist>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Playlist>("User not found");
        
        var playlist  = await context.Playlists.FirstOrDefaultAsync(s => s.Id == dto.Id);
        if (playlist is null) return new Fail<Playlist>($"Song with ID: {dto.Id} not found");
        
        return new Success<Playlist>(playlist);
    }
    
    

    public async Task<OperationResult<Playlist>> CreateAsync(PlaylistDto dto)
    {
        // Ensure the user is logged in and exists in the system
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Playlist>("User not found");

        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext.User.Identity.Name!);
        if (user is null) return new Fail<Playlist>("User not found");

        // Check if a playlist with the same name already exists
        var existingPlaylist = await context.Playlists
            .FirstOrDefaultAsync(p => p.Name == dto.Name && p.OwnerId == user.Id);
        
        if (existingPlaylist is not null) 
            return new Fail<Playlist>("Playlist with similar configuration already exists");

        // Create the playlist
        var playlist = new Playlist
        {
            IsShuffled = dto.IsShuffled,
            OwnerId = user.Id
        };

        await context.Playlists.AddAsync(playlist);
        await context.SaveChangesAsync();

        return new Success<Playlist>(playlist);
    }

    public async Task<OperationResult<Playlist>> UpdateAsync(PlaylistDto dto)
    {
        // Ensure the user is logged in and exists in the system
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Playlist>("User not found");

        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext.User.Identity.Name!);
        if (user is null) return new Fail<Playlist>("User not found");

        // Fetch the playlist to update
        var existingPlaylist = await context.Playlists
            .Include(p => p.Songs) // Include Songs for easier modification
            .FirstOrDefaultAsync(p => p.Id == dto.Id && p.OwnerId == user.Id);
        
        if (existingPlaylist is null) 
            return new Fail<Playlist>($"Playlist with ID {dto.Id} not found or you don't have permission to edit it");

        // Update playlist properties
        existingPlaylist.Name = dto.Name;
        existingPlaylist.IsShuffled = dto.IsShuffled;
        context.Playlists.Update(existingPlaylist);
        await context.SaveChangesAsync();

        return new Success<Playlist>(existingPlaylist);
    }

    public async Task<OperationResult<Playlist>> DeleteAsync(PlaylistDto dto)
    {
        // Ensure the user is logged in and exists in the system
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Playlist>("User not found");

        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext.User.Identity.Name!);
        if (user is null) return new Fail<Playlist>("User not found");

        // Find the playlist to delete
        var existingPlaylist = await context.Playlists
            .FirstOrDefaultAsync(p => p.Id == dto.Id && p.OwnerId == user.Id);

        if (existingPlaylist is null)
            return new Fail<Playlist>($"Playlist with ID {dto.Id} not found or you don't have permission to delete it");

        // Remove the playlist
        context.Playlists.Remove(existingPlaylist);
        await context.SaveChangesAsync();

        return new Success<Playlist>(existingPlaylist);
    }
}