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
public class SongsController(EntityCrudExecutor<SongDto, Song> executor): Controller
{
    [HttpGet]
    public async Task<IActionResult> Get ()
    {
        return await executor.Get();
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        return await executor.Get(new SongDto { Id = id });
    }
    
    [HttpPost]
    public async Task<IActionResult> Post(SongDto dto)
    {
        return await executor.Create(dto);
    }
    
    [HttpPut]
    public async Task<IActionResult> Put(SongDto dto)
    {
        return await executor.Update(dto);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await executor.Delete(new SongDto { Id = id });
    }

}