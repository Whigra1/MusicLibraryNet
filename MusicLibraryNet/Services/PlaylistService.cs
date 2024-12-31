using System.Linq.Expressions;
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
    
    public async Task<OperationResult<List<Playlist>>> GetAsync(Expression<Func<Playlist, bool>>? filter = null)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<List<Playlist>>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<List<Playlist>>("User not found");
        
        
        var song = await context.Playlists
            .Where(s => s.OwnerId == user.Id)
            .Where(filter ?? (_ => true))
            .ToListAsync();
        return new Success<List<Playlist>>(song);
    }
    
    public async Task<OperationResult<Playlist>> GetAsync(PlaylistDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Playlist>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Playlist>("User not found");

        var playlistQuery = context.Playlists
            .Include(p => p.PlaylistSongs)
                .ThenInclude(p => p.Song)
            .Where(p => p.OwnerId == user.Id);
        
        if (dto.Id > 0) playlistQuery = playlistQuery.Where(p => p.Id == dto.Id);
        if (!string.IsNullOrEmpty(dto.Name)) playlistQuery = playlistQuery.Where(p => p.Name.Equals(dto.Name, StringComparison.CurrentCultureIgnoreCase));
        
        var playlist = await playlistQuery.FirstOrDefaultAsync();
        if (playlist is null) return new Fail<Playlist>($"Playlist with ID: {dto.Id} not found or not accessible");

        playlist.PlaylistSongs.ForEach(pS => { pS.Song.Owner = null; });
        
        var playlistR = new Playlist
        {
            Id = playlist.Id,
            IsShuffled = playlist.IsShuffled,
            Name = playlist.Name,
            OwnerId = playlist.OwnerId,
            PlaylistSongs = playlist.PlaylistSongs
        };
        
        return new Success<Playlist>(playlistR);
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
            Name = dto.Name,
            IsShuffled = dto.IsShuffled,
            OwnerId = user.Id,
            PlaylistSongs = []
        };

        await context.Playlists.AddAsync(playlist);
        await context.SaveChangesAsync();

        return new Success<Playlist>(new Playlist
        {
            Id = playlist.Id,
            Name = playlist.Name
        });
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
            .Include(p => p.PlaylistSongs) // Include Songs for easier modification
                .ThenInclude(ps => ps.Song)
            .FirstOrDefaultAsync(p => p.Id == dto.Id && p.OwnerId == user.Id);
        
        if (existingPlaylist is null) 
            return new Fail<Playlist>($"Playlist with ID {dto.Id} not found or you don't have permission to edit it");

        // Update playlist properties
        existingPlaylist.Name = dto.Name;
        existingPlaylist.IsShuffled = dto.IsShuffled;
        existingPlaylist.PlaylistSongs = (dto.Songs ?? []).Select(s => new PlaylistSong
        {
            SongId = (int) s.Id!,
            Order = s.Order,
        }).ToList();
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