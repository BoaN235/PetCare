namespace PetCare.Core;

public interface IGameLogData
{
    void Add(string message, string userPrompt);
    void Add(string message);
    void Add(GameLogEntry entry);
    
    List<GameLogEntry> Entries { get; }
 }