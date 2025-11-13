namespace PetCare.Core;

public interface IGameLog : IGameLogData, IDataObject
{
    string GetLogText();
    List<GameLogEntry> GetChatLog(ChatIdEnum chatId);
}