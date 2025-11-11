namespace PetCare.Core;

public class GameLogEntry
{
    public GameLogEntry() { }
    public GameLogEntry(string Line, string userprompt)
    {
        Message = Line;
        UserPrompt = userprompt;
    }

    public ChatIdEnum ChatId { get; set; }
    public DateTime MessageTime { get; } = DateTime.Now;
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
