namespace MusicLibraryNet.Dto.Chat;

public class ChatDto
{
    public int? Id { get; set; }
    public List<MessageDto> Messages { get; set; }
    public string Name { get; set; }
    public int CreatorId { get; set; }
}