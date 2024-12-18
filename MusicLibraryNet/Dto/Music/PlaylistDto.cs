namespace MusicLibraryNet.Dto.Music;

public class PlaylistDto
{
    public int Id { get; set; }

    public string Name { get; set; }
    public bool IsShuffled { get; set; }
    public int OwnerId { get; set; }
}