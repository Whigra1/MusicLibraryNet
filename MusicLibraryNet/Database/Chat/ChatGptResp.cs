namespace MusicLibraryNet.Database.Chat;

public class ChatGptResp
{
    public int Id { get; set; }
    public int RespondMessageId { get; set; }
    public Message RespondMessage { get; set; }
    public string Responce { get; set; }
}