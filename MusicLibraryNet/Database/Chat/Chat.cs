using MusicLibraryNet.Database.Auth;

namespace MusicLibraryNet.Database.Chat;

public class Chat
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Message> Messages { get; set; }
    public MusicUser Creator { get; set; }
    public int CreatorId { get; set; }
}