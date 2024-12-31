using System.Text;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;

public class ChatGtpResponse
{
    public class DataResponse
    {
        public string ActionName { get; set; }
        public string Where { get; set; }
        public Dictionary<string, string> Params { get; set; }
    }

    public string Type { get; set; }
    public string TextResponse { get; set; } = "I don't know what you mean";
    public DataResponse Data { get; set; }
}

public class ChatGptService (OpenAIClient openAiClient)
{
    public async Task<string> AskChatGptAsync(string prompt)
    {
        var chatClient = openAiClient.GetChatClient("gpt-4o");
        var systemPrompt = "You are a music library web application assistant. You can answer questions about music, songs, artists, albums, and playlists." +
                           "But also you can handle navigation requests like \"open library\" or \"open playlist <name>\"" +
                           "User can navigate only on 7 pages: home, library, profile, playlists, playlist, playlist editing, chat(Alfred)" +
                           "You should always return plain json responses with type and your answer for example if user want to navigate somewhere you should return json like this:" +
                           "{\"type\": \"navigation\", \"data\": { \"where\": \"playlist\" or \"home\" or \"library\", \"params\": {} }, \"textResponse\": your response } " +
                           "or { \"type\": \"question\", \"data\": {}, \"textResponse\": your response } "+
                           "or { \"type\": \"action\", \"data\": { \"actionName\": PLAY_MUSIC or PLAY_PLAYLIST, \"params\": {} }, \"textResponse\": your response }" +
                           "Also user can ask for suggestions like \"suggest me songs about happy-sounding pop songs\"." +
                           "You can also talk about some basic questions like \" How are you?\" \"What are you doing?\" etc."+
                           "If user asks you about stuff that don't connect to project you should return \"I don't answer questions about other topics\".";

        // Example: Asking for chord progression suggestions
    
        // Create chat message for the assistant
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage(prompt)
        };

        var sBuilder = new StringBuilder(100);
        
        var resp = await chatClient.CompleteChatAsync(messages);
        foreach (var completion in resp.Value.Content)
        {
            sBuilder.Append(completion.Text);
        }

        return sBuilder.ToString();
    }
}