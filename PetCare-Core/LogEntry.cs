namespace PetCare.Core;

public class GameLogEntry
{
    public GameLogEntry() { }
    public GameLogEntry(string Line)
    {
        Message = Line;
    }

    public ChatIdEnum ChatId { get; set; }
    public DateTime MessageTime { get; } = DateTime.Now;
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
