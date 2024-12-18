using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicLibraryNet.Database.Auth;
using MusicLibraryNet.Database.Music;

namespace MusicLibraryNet.Database;

public class ApplicationDbContext(DbContextOptions options)
    : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>(options)
{
    public DbSet<File> Files { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
}