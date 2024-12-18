using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicLibraryNet.Database.Music;
using MusicLibraryNet.Dto.Music;
using MusicLibraryNet.Services;

namespace MusicLibraryNet.Controllers.Music;


[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class PlaylistController(
    EntityCrudExecutor<PlaylistDto, Playlist> executor
): Controller
{
    [HttpGet]
    public async Task<IActionResult> Get ()
    {
        return await executor.Get();
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        return await executor.Get(new PlaylistDto { Id = id });
    }
    
    [HttpPost]
    public async Task<IActionResult> Post(PlaylistDto dto)
    {
        return await executor.Create(dto);
    }
    
    [HttpPut]
    public async Task<IActionResult> Put(PlaylistDto dto)
    {
        return await executor.Update(dto);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await executor.Delete(new PlaylistDto { Id = id });
    }

}