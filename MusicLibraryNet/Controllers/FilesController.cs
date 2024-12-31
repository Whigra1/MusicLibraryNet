using Microsoft.AspNetCore.Mvc;
using MusicLibraryNet.Database.Music;
using MusicLibraryNet.Dto;
using MusicLibraryNet.Dto.Music;
using MusicLibraryNet.Services;
using MusicLibraryNet.Services.Abstractions;

namespace MusicLibraryNet.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class FilesController(
    ISongService<SongDto, Song> songService,
    EntityCrudExecutor<FileDto, Database.File> executor
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Get() => await executor.Get();
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) => await executor.Get(new FileDto { Id = id });

    [HttpPost]
    public async Task<IActionResult> Post(FileDto dto) => await executor.Create(dto);
    
    [HttpPut]
    public async Task<IActionResult> Put(FileDto dto) => await executor.Update(dto);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => await executor.Delete(new FileDto { Id = id });
} 