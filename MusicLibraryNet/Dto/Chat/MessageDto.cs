namespace MusicLibraryNet.Dto.Chat;

public class MessageDto
{
    public int? Id { get; set; }
    public string Text { get; set; }
    public int ChatId { get; set; }
    public int UserId { get; set; }
}