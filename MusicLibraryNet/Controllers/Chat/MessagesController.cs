using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicLibraryNet.Database.Chat;
using MusicLibraryNet.Dto.Chat;
using MusicLibraryNet.Services;
using MusicLibraryNet.Services.Abstractions;

namespace MusicLibraryNet.Controllers.Chat;

[Authorize]
[ApiController]
[Route("/api/v1/[controller]")]
public class MessagesController(
    EntityCrudExecutor<MessageDto, Message> executor,
    IMessageService<MessageDto, Message> messageService
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Get() => await executor.Get();
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) => await executor.Get(new MessageDto { Id = id });

    [HttpPost]
    public async Task<IActionResult> Post(MessageDto dto) {
        var create = await messageService.CreateAsync(dto);
        return create switch
        {
            Fail<Message> fail => BadRequest(fail.Message),
            Success<Message> success => Ok(success.Value.Text),
            _ => BadRequest("Unknown error")
        };
    }
    
    [HttpPut]
    public async Task<IActionResult> Put(MessageDto dto) => await executor.Update(dto);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => await executor.Delete(new MessageDto { Id = id });
}