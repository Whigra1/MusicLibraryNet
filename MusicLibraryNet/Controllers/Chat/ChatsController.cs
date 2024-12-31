using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicLibraryNet.Dto;
using MusicLibraryNet.Dto.Chat;
using MusicLibraryNet.Services;

namespace MusicLibraryNet.Controllers.Chat;

[Authorize]
[ApiController]
[Route("/api/v1/[controller]")]
public class ChatsController(
    EntityCrudExecutor<ChatDto, Database.Chat.Chat> executor    
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Get() => await executor.Get();
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) => await executor.Get(new ChatDto { Id = id });

    [HttpPost]
    public async Task<IActionResult> Post(ChatDto dto) => await executor.Create(dto);
    
    [HttpPut]
    public async Task<IActionResult> Put(ChatDto dto) => await executor.Update(dto);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => await executor.Delete(new ChatDto { Id = id });
}