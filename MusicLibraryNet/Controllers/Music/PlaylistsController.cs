using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicLibraryNet.Database.Music;
using MusicLibraryNet.Dto.Music;
using MusicLibraryNet.Services;
using MusicLibraryNet.Services.Abstractions;

namespace MusicLibraryNet.Controllers.Music;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class PlaylistsController(
    IPlaylistService<PlaylistDto, Playlist> playlistService,
    EntityCrudExecutor<PlaylistDto, Playlist> executor
): Controller
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? name = null)
    {
        return await (string.IsNullOrEmpty(name) ? executor.Get() : executor.Get(new PlaylistDto { Name = name }));
    }
    
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        return await executor.Get(new PlaylistDto { Id = id });
    }
    
    [HttpGet("{id:int}/Songs/")]
    public async Task<IActionResult> GetSongs(int id)
    {
        return await playlistService.GetAsync(new PlaylistDto { Id = id }) switch
        {
            Success<Playlist> p => Ok(p.Value.PlaylistSongs
                .Select(ps => new PlaylistSongDto
                {
                    Id = ps.Song.Id,
                    OwnerId = ps.Song.OwnerId,
                    Order = ps.Order,
                    Description = ps.Song.Description,
                    Title = ps.Song.Title,
                    Artist = ps.Song.Artist
                }).ToList()),
            _ => BadRequest("Playlist not found")
        };
    }
    
    [HttpPost]
    public async Task<IActionResult> Post(PlaylistDto dto)
    {
        return await executor.Create(dto);
    }
    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put([FromQuery] int id, PlaylistDto dto)
    {
        return await executor.Update(dto);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await executor.Delete(new PlaylistDto { Id = id });
    }

}