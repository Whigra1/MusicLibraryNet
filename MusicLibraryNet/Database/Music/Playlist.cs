using MusicLibraryNet.Database.Auth;

namespace MusicLibraryNet.Database.Music;

public class Playlist
{
    public int Id { get; set; }

    public string Name { get; set; }
    
    public bool IsShuffled { get; set; }
    public List<PlaylistSong> PlaylistSongs { get; set; }
    public int OwnerId { get; set; }
    public MusicUser Owner { get; set; }
}