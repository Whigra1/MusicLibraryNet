using MusicLibraryNet.Database.Auth;
using MusicLibraryNet.Database.Music;

namespace MusicLibraryNet.Database;

public class File
{
    public int Id { get; set; }
    public string Path { get; set; }

    public int SongId { get; set; }
    public Song Song { get; set; }
}