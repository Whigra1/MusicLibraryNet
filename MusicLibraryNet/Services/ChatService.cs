using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicLibraryNet.Database;
using MusicLibraryNet.Database.Auth;
using MusicLibraryNet.Database.Chat;
using MusicLibraryNet.Dto.Chat;
using MusicLibraryNet.Services.Abstractions;

namespace MusicLibraryNet.Services;

public class ChatService(
    ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor,
    UserManager<MusicUser> userManager
) : IChatService<ChatDto, Chat>
{
    public async Task<OperationResult<Chat>> GetAsync(ChatDto dto)
    {
        
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Chat>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Chat>("User not found");

        
        var chat = await context.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == dto.Id);
        
        if (chat is null) return new Fail<Chat>("Chat not found");

        if (chat.CreatorId != user.Id) return new Fail<Chat>("Chat not found or not accessible");
        
        return new Success<Chat>(new Chat
        {
            Id = chat.Id,
            CreatorId = user.Id,
            Name = chat.Name,
            Messages = chat.Messages.Select(m => new Message
            {
                Id = m.Id,
                ChatId = m.ChatId,
                Text = m.Text,
                UserId = m.UserId
            }).ToList()
        });
        
    }

    public async Task<OperationResult<List<Chat>>> GetAsync(Expression<Func<Chat, bool>>? queryBuilder = null)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<List<Chat>>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<List<Chat>>("User not found");

        
        var chats = await context.Chats
            .Where(queryBuilder ?? (_ => true))
            .Where(c => c.CreatorId == user.Id)
            .Include(c => c.Messages)
            .ToListAsync();
        
        return new Success<List<Chat>>(chats.Select(chat => new Chat
        {
            Id = chat.Id,
            CreatorId = user.Id,
            Name = chat.Name,
            Messages = chat.Messages.Select(m => new Message
            {
                Id = m.Id,
                ChatId = m.ChatId,
                Text = m.Text,
                UserId = m.UserId,
            }).ToList()
        }).ToList());
    }

    public async Task<OperationResult<Chat>> CreateAsync(ChatDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Chat>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Chat>("User not found");

        var chat = new Chat
        {
            Name = dto.Name,
            CreatorId = user.Id,
            Messages = [
                new Message
                {
                    UserId = (await userManager.FindByNameAsync("ChatGPT"))!.Id,
                    Text = "How can I help you ?",
                    Date = DateTime.UtcNow,
                }
            ]
        };
        
        await context.AddAsync(chat);

        await context.SaveChangesAsync();

        return new Success<Chat>(new Chat
        {
            Id = chat.Id,
            Name = chat.Name
        });

    }

    public async Task<OperationResult<Chat>> UpdateAsync(ChatDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Chat>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Chat>("User not found");

        var chat = context.Chats.FirstOrDefault(c => c.Id == dto.Id);

        
        if (chat is null || chat.CreatorId != user.Id)
        {
            return new Fail<Chat>("Chat not found or not accessible");
        }
        
        chat.Name = dto.Name;
        
        context.Update(chat);

        await context.SaveChangesAsync();

        return new Success<Chat>(chat);
    }

    public async Task<OperationResult<Chat>> DeleteAsync(ChatDto dto)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.Name is null)
            return new Fail<Chat>("User not found");
        
        var user = await userManager.FindByNameAsync(httpContextAccessor.HttpContext!.User.Identity!.Name);
        if (user is null) return new Fail<Chat>("User not found");

        var chat = context.Chats.FirstOrDefault(c => c.Id == dto.Id);
        
        if (chat is null || chat.CreatorId != user.Id)
        {
            return new Fail<Chat>("Chat not found or not accessible");
        }
        
        
        context.Remove(chat);
        await context.SaveChangesAsync();
        return new Success<Chat>(chat);
    }
}