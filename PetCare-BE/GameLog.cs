using PetCare.Core;
using System.Text;
using System.Linq;

namespace PetCare.BE;

public class GameLog : BaseDataObject<List<GameLogEntry>>, IGameLog
{

    public GameLog() : base("log_data.json")
    {
    }

    public List<GameLogEntry> Entries => base.DataObj;

    public void Add(string message)
    {
        DataObj.Add(new GameLogEntry(message) { ChatId = ChatIdEnum.System });
    }
    public void Add(GameLogEntry entry)
    {
        DataObj.Add(entry);
    }
    public string GetLogText()
    {
        StringBuilder log_string = new();
        foreach (var entry in DataObj)
        {
            log_string.AppendLine("Owner: " + entry.UserPrompt + "\n");
            log_string.AppendLine($"pet:" + entry.Message + "\n");
        }
        return log_string.ToString();
    }

    public List<GameLogEntry> GetChatLog(ChatIdEnum chatId)
    {
        return DataObj
            .Where(entry => entry.ChatId == chatId)
            .OrderByDescending(o => o.MessageTime)
            .ToList();
    }
}


