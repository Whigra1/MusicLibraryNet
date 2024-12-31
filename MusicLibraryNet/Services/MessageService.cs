using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicLibraryNet.Database;
using MusicLibraryNet.Database.Auth;
using MusicLibraryNet.Database.Chat;
using MusicLibraryNet.Dto.Chat;
using MusicLibraryNet.Services.Abstractions;

namespace MusicLibraryNet.Services;

public class MessageService(
    ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor,
    UserManager<MusicUser> userManager,
    ChatGptService chatGptService
): IMessageService<MessageDto, Message>
{
    public async Task<OperationResult<Message>> GetAsync(MessageDto dto)
    {
       return new Fail<Message>("Not implemented");
    }

    public async Task<OperationResult<List<Message>>> GetAsync(Expression<Func<Message, bool>>? queryBuilder = null)
    {
        return new Fail<List<Message>>("not implemented");
    }

    public async Task<OperationResult<Message>> CreateAsync(MessageDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Message>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Message>("User not found");

        var chat = await context.Chats.FirstOrDefaultAsync(c => c.Id == dto.ChatId);
        if (chat is null || chat.CreatorId != user.Id) return new Fail<Message>("Chat not found or not accessible");
        
        var message = new Message
        {
            Text = dto.Text,
            ChatId = dto.ChatId,
            UserId = user.Id,
            Date = DateTime.UtcNow,
        };
        
        await context.Messages.AddAsync(message);
        
        var chatGptRawResp = await chatGptService.AskChatGptAsync(message.Text);

        await context.ChatGptResps.AddAsync(new ChatGptResp
        {
            RespondMessage = message,
            Responce = chatGptRawResp
        });
        
        
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
        
        var chatRespMessage = new Message
        {
            Text = chatGptResp.TextResponse ?? "I don't know what to say.",
            ChatId = dto.ChatId,
            UserId = (await userManager.FindByNameAsync("ChatGPT"))!.Id,
            Date = DateTime.UtcNow,
        };
        
        await context.Messages.AddAsync(chatRespMessage);
        await context.SaveChangesAsync();
        return new Success<Message>(new Message
        {
            Text = JsonSerializer.Serialize(chatGptResp, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
            ChatId = chatRespMessage.ChatId,
            UserId = chatRespMessage.UserId,
            Date = chatRespMessage.Date,
            Id = chatRespMessage.Id
        });
    }

    public async Task<OperationResult<Message>> UpdateAsync(MessageDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Message>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Message>("User not found");

        var chat = await context.Chats.FirstOrDefaultAsync(c => c.Id == dto.ChatId);
        if (chat is null || chat.CreatorId != user.Id) return new Fail<Message>("Chat not found or not accessible");
        
        var message = await context.Messages.FirstOrDefaultAsync(m => m.Id == dto.Id);
        
        if (message is null || message.UserId != user.Id ) return new Fail<Message>("Message not found or not accessible");
        
        message.Text = dto.Text;
        
        context.Messages.Update(message);

        await context.SaveChangesAsync();

        return new Success<Message>(message);
    }

    public async Task<OperationResult<Message>> DeleteAsync(MessageDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Message>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Message>("User not found");

        var chat = await context.Chats.FirstOrDefaultAsync(c => c.Id == dto.ChatId);
        if (chat is null || chat.CreatorId != user.Id) return new Fail<Message>("Chat not found or not accessible");
        
        var message = await context.Messages.FirstOrDefaultAsync(m => m.Id == dto.Id);
        
        if (message is null || message.UserId != user.Id ) return new Fail<Message>("Message not found or not accessible");
        
        context.Messages.Remove(message);

        await context.SaveChangesAsync();

        return new Success<Message>(message);
    }
}