namespace PetCare.Core;

public interface IAiModel
{
    Task InitModel();
    Task<GameLogEntry> RunModel(ChatIdEnum chatId, string userInput, bool isStateful = true);
    Task<GameLogEntry> RunModelStateless(ChatIdEnum chatId, string userInput);
    Task<GameLogEntry> RunModel(ChatIdEnum chatId, string userinput);
}