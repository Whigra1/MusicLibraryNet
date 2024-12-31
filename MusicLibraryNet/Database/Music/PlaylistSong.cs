namespace MusicLibraryNet.Database.Music;

public class PlaylistSong
{
    public int Id { get; set; }
    public int Order { get; set; }
    public int SongId { get; set; }
    public Song Song { get; set; }
}