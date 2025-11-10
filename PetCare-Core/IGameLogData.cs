namespace PetCare.Core;

public interface IGameLogData
{
    void Add(string message);
    void Add(GameLogEntry entry);
    List<GameLogEntry> Entries { get; }
}

//public static class Log
//{
//    private static readonly string fileName = "log_data.json";
//    private static readonly string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

//    // Use List for easier appending
//    public static List<LogEntry> Entries { get; private set; } = new();

//    public static void Add(string message)
//    {
//        Entries.Add(new LogEntry(message));
//    }

//    public static async Task Import_Log()
//    {
//        try
//        {
//            if (!File.Exists(filePath))
//            {
//                Debug.WriteLine("⚠️ No existing log file found, starting fresh.");
//                Entries = new List<LogEntry>();
//                return;
//            }

//            string json = await File.ReadAllTextAsync(filePath);
//            var loadedEntries = JsonSerializer.Deserialize<List<LogEntry>>(json);

//            if (loadedEntries != null)
//                Entries = loadedEntries;

//            Debug.WriteLine($"✅ Loaded {Entries.Count} log entries.");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"❌ Error loading log: {ex.Message}");
//        }
//    }
//    public static async Task Export_Log()
//    {
//        try
//        {
//            string json = JsonSerializer.Serialize(Entries, new JsonSerializerOptions
//            {
//                WriteIndented = true
//            });

//            await File.WriteAllTextAsync(filePath, json);
//            Debug.WriteLine($"✅ Log saved to: {filePath}");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"❌ Error saving log: {ex.Message}");
//        }
//    }

//    public static void Clear()
//    {
//        Entries.Clear();
//    }
//}
