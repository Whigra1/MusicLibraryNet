using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicLibraryNet.Database.Music;
using MusicLibraryNet.Dto;
using MusicLibraryNet.Dto.Music;
using MusicLibraryNet.Services;
using MusicLibraryNet.Services.Abstractions;
using File = MusicLibraryNet.Database.File;

namespace MusicLibraryNet.Controllers.Music;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class SongsController(
    ISongService<SongDto, Song> songService,
    EntityCrudExecutor<SongDto, Song> executor,
    EntityCrudExecutor<FileDto, File> fileExecutor
): Controller
{
    [HttpGet]
    public async Task<IActionResult> Get ([FromQuery] string? name = null) =>
        await (string.IsNullOrEmpty(name) ? executor.Get() : executor.Get(new SongDto { Title = name}));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) => await executor.Get(new SongDto { Id = id });

    [HttpPost]
    public async Task<IActionResult> Post(SongDto dto) => await executor.Create(dto);
    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(SongDto dto) => await executor.Update(dto);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => await executor.Delete(new SongDto { Id = id });
    
    // Files
    [HttpPost("{songId:int}/Files/")]
    public async Task<IActionResult> Post(int songId, FileDto dto)
    {
        dto.SongId = songId;
        return await songService.GetAsync(new SongDto { Id = songId }) switch
        {
            Success<Song> => await fileExecutor.Create(dto),
            _ => BadRequest("Song not found")
        };
    }
    
    [HttpGet("{songId:int}/Files/")]
    public async Task<IActionResult> GetSongFiles(int songId)
    {
        return await songService.GetAsync(new SongDto { Id = songId }) switch
        {
            Success<Song> => await fileExecutor.Get(f => f.SongId == songId),
            _ => BadRequest("Song not found")
        };
    }
}