using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicLibraryNet.Database;
using MusicLibraryNet.Utils;

namespace MusicLibraryNet.Controllers;
[Authorize]
[ApiController]
[Route("/api/v1/[controller]")]
public class AlfredController (
    ApplicationDbContext context,
    ChatGptService chatGptService,
    IWebHostEnvironment env
) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] string message)
    {
        var chatGptRawResp = await chatGptService.AskChatGptAsync(message);
        ChatGtpResponse chatGptResp;
        try
        {
            if (chatGptRawResp.StartsWith("```json"))
            {
                chatGptRawResp = chatGptRawResp
                    .Replace("```json", "")
                    .Replace("```", "");
            }
            chatGptResp = JsonSerializer.Deserialize<ChatGtpResponse>(chatGptRawResp , new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new ChatGtpResponse();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            chatGptResp = new ChatGtpResponse();
        }

        return Ok(chatGptResp);
    }

    [HttpGet("model")]
    public Task<IActionResult> GetModel()
    {
        var bytes = System.IO.File.OpenRead(env.ContentRootPath + "/Assets/final.glb");
        return Task.FromResult<IActionResult>(File(bytes, "application/octet-stream"));
    }
}