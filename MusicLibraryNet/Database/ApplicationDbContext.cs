using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicLibraryNet.Database.Auth;
using MusicLibraryNet.Database.Chat;
using MusicLibraryNet.Database.Music;

namespace MusicLibraryNet.Database;

public class ApplicationDbContext(DbContextOptions options)
    : IdentityDbContext<MusicUser, MusicRole, int>(options)
{
    public DbSet<File> Files { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<Chat.Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ChatGptResp> ChatGptResps { get; set; }
}