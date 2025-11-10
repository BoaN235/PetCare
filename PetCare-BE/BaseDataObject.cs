using System.Diagnostics;
using System.Text.Json;

namespace PetCare.BE;

public abstract class BaseDataObject<T> where T : class, new()
{
    private readonly string _systemFilePath;

    public BaseDataObject(string filePath)
    {
        this._systemFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, filePath);
    }

    protected T DataObj { get; set; } = new T();

    public virtual async Task Import()
    {
        try
        {
            if (!File.Exists(_systemFilePath))
            {
                Debug.WriteLine($"⚠️ No existing {this.GetType().Name} file found, starting fresh.");
                DataObj = new();
                return;
            }

            string json = await File.ReadAllTextAsync(_systemFilePath);
            DataObj = JsonSerializer.Deserialize<T>(json) ?? new();


            Debug.WriteLine($"✅ Loaded {this.GetType().Name} entries.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error loading {this.GetType().Name}: {ex.Message}");
        }
    }

    public virtual async Task Export()
    {
        try
        {
            string json = JsonSerializer.Serialize(DataObj, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_systemFilePath, json);
            Debug.WriteLine($"✅ {this.GetType().Name} saved to: {_systemFilePath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error saving {this.GetType().Name}: {ex.Message}");
        }
    }

    public virtual async Task Clear()
    {
        if (File.Exists(_systemFilePath))
        {
            File.Delete(_systemFilePath);
        }
        DataObj = new();
        await Export();
    }
}


