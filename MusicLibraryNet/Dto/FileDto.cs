namespace MusicLibraryNet.Dto;

public class FileDto
{
    public int? Id { get; set; }
    public string Path { get; set; }
    public int? SongId { get; set; }
    public IFormFile? FileData { get; set; }
}