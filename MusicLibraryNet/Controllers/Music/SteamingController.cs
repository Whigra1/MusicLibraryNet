using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicLibraryNet.Database;
using MusicLibraryNet.Providers;

namespace MusicLibraryNet.Controllers.Music;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class StreamingController(
    ApplicationDbContext context,
    MusicStoreProvider musicStoreProvider
) : ControllerBase
{
    [HttpGet("{fileId:int}")]
    public IActionResult StreamAudio(int fileId)
    {
        var file = context.Files.FirstOrDefault(f => f.Id == fileId);
        if (file is null)
        {
            return NotFound("File not found");
        }
      
        // Stream the file with support for range requests (important for streaming)
        var stream = musicStoreProvider.GetFileStream(file.Path);
        if (stream is null)
        {
            return BadRequest("File not found or cannot be streamed");
        }
        return File(stream, "audio/mpeg", enableRangeProcessing: true);
    }
}