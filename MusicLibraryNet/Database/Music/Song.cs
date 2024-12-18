using MusicLibraryNet.Database.Auth;

namespace MusicLibraryNet.Database.Music;

public class Song
{
    public int Id { get; set; }
    public string Artist { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public MusicUser Owner { get; set; }
    public int OwnerId { get; set; }
    public List<File> Files { get; set; } // One song can have multiple files
}