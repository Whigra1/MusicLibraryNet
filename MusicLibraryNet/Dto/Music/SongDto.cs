using MusicLibraryNet.Database.Auth;

namespace MusicLibraryNet.Dto.Music;

public class SongDto
{
    public int? Id { get; set; }
    public string Artist { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int? OwnerId { get; set; }
    
    public List<IFormFile>? Files { get; set; }
}