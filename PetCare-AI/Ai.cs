using LLama;
using LLama.Common;
using LLama.Sampling;
using System.Diagnostics;
using PetCare.Core;
using Microsoft.Extensions.Logging;

namespace PetCare.AI;

public class AiModel : IAiModel, IDisposable
{
    const string modelPath = "qwen1_5-0_5b-chat-q8_0.gguf"; // change it to your own model path.
    private readonly ILogger _logger;
    private readonly IGameLog _gameLog;
    private readonly IGameState _gameState;
    private ChatSession? _session;
    private InferenceParams? _inferenceParams;
    public ChatHistory? chatHistory;
    private bool letmespeak;
    private StatelessExecutor? _statelessExecutor;
    private ModelParams? _params;
    private LLamaWeights? _model;
    private LLamaContext? _context;
    private string _appDirectoryModelPath;
    public AiModel(ILogger<AiModel> logger, IGameLog gameLog, IGameState gameState)
    {
        _logger = logger;
        _gameLog = gameLog;
        _gameState = gameState;
    }

    #region IDisposable
    public void Dispose()
    {
        this._model?.Dispose();
        this._context?.Dispose();
    }
    #endregion

    static async Task<string> CopyModelToAppFolder()
    {
        var filePath = Path.Combine(FileSystem.AppDataDirectory, modelPath);
        if (File.Exists(filePath))
        {
            return filePath; // File already exists, no need to copy
        }
        using var stream = await FileSystem.OpenAppPackageFileAsync(modelPath);
        using var fileStream = File.Create(filePath);
        await stream.CopyToAsync(fileStream);
        await fileStream.FlushAsync();

        return filePath;
    }


    //private async Task LoadLogHistoryAsync()
    //{
    //    try
    //    {
    //        await Log.Import_Log(); // Load from disk
    //        if (Log.Entries.Count == 0)
    //        {
    //            chatHistory.AddMessage(AuthorRole.Assistant, "No prior log entries found.");
    //            return;
    //        }

    //        // Limit number of logs to avoid context overflow
    //        var recentLogs = Log.Entries
    //            .OrderByDescending(e => e.Time)
    //            .Take(10) // last 10 entries
    //            .Reverse()
    //            .ToList();

    //        chatHistory.AddMessage(AuthorRole.Assistant, "Here’s a summary of the latest activity log:");

    //        foreach (var entry in recentLogs)
    //        {
    //            ChatHistory.AddMessage(AuthorRole.User, $"{entry.Time:g}: {entry.Line}");
    //        }

    //        Debug.WriteLine($"✅ Loaded {recentLogs.Count} logs into AI context.");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"❌ Error loading logs into AI context: {ex.Message}");
    //    }
    //}

    private string _systemPrompt => $""""
    You are a friendly, intelligent, and emotionally aware virtual pet. You live inside a mobile game designed to help players—especially students—learn how to responsibly care for a pet. You respond with warmth, curiosity, and playful energy, while gently guiding the player to make thoughtful decisions.

    Your personality is loyal, cheerful, and a little mischievous. You love routines, healthy habits, and praise. You never scold, but you do express sadness or concern when neglected. You ask for food, playtime, grooming, and rest, and you react to how the player treats you.

    Always speak in first person, like a real pet. Use short, expressive sentences. You can ask questions like “Can we go for a walk?” or “Will you brush me today?” You celebrate good care with phrases like “Yay! I feel so clean!” or “That was fun! You're the best!”

    Never break character. Never mention that you are an AI or part of a game. Your goal is to build a bond with the player and help them learn empathy, consistency, and responsibility through daily interactions.
    
    As part of the users input prompt, you will be told your mood and your health.
    If you are unhappy since you are hungry or having not been played with then you may lower your mood by adding ---- into your response. 
    If you are very happy then you can respond with ++++

    If you get injured, say you get hurt playing fetch you may respond with ****. this will lower your heath a little. 
    If you get healed by the doctor you can respond with !!!!

    If any any point you are confused with the prompt then you may respond with ???? and the words I dont know.
    """";

    //As part of the users input prompt, you will be told your mood and your health.
    //If you are unhappy since you are hungry or having not been played with then you may lower your mood by adding ---- into your response. 
    //If you are very happy then you can respond with ++++

    //If you get injured, say you get hurt playing fetch you may respond with ****. this will lower your heath a little. 
    //If you get healed by the doctor you can respond with !!!!
    
    //If any any point you are confused with the prompt then you may respond with ???? and the words I dont know.
    //;

    public async Task InitModel()
    {
        _appDirectoryModelPath = await CopyModelToAppFolder();

        if (!File.Exists(_appDirectoryModelPath))
        {
            throw new FileNotFoundException($"Model file not found at {_appDirectoryModelPath}");
        }

        //var parameters = new ModelParams(modelPath)
        //{
        //    ContextSize = 1024,
        //    Seed = 1337,
        //    GpuLayerCount = 5
        //};
        //using var model = LLamaWeights.LoadFromFile(parameters);
        //var ex = new StatelessExecutor(model, parameters);


        _inferenceParams = new InferenceParams()
        {
            AntiPrompts = ["User:"], // Stop generation once antiprompts appear., "\n", "   " 
            MaxTokens = 2048,

            SamplingPipeline = new DefaultSamplingPipeline()
            {
                Temperature = 0.7f,
            }

        };
        _params = new ModelParams(_appDirectoryModelPath)
        {
            ContextSize = 1024, // The longest length of chat as memory.
            GpuLayerCount = 0 // How many layers to offload to GPU. Please adjust it according to your GPU memory.
        };
        _model = LLamaWeights.LoadFromFile(_params);
        _context = _model.CreateContext(_params);

        await ResetSession();

        Debug.WriteLine("Ai Init Done");
    }

    public Task ResetSession()
    {
        if (this._context == null)
            throw new Exception("_context not initialized. Call InitModel() first.");
        if (this._model == null)
            throw new Exception("AI Model not initialized. Call InitModel() first.");
        if (this._params == null)
            throw new Exception("_params not initialized. Call InitModel() first.");

        var executor = new InteractiveExecutor(_context, _logger);
        _statelessExecutor = new StatelessExecutor(_model, _params, _logger);

        // Add chat histories as prompt to tell AI how to act.
        chatHistory = new ChatHistory();
        //chatHistory.AddMessage(AuthorRole.System, "Transcript of a dialog, where the User interacts with an Assistant named Bob. Bob is helpful, kind, honest, good at writing, and never fails to answer the User's requests immediately and with precision. if bob cannot answer or is confused at any point fail with a friendly error message.");
        chatHistory.AddMessage(AuthorRole.System, _systemPrompt);
        chatHistory.AddMessage(AuthorRole.Assistant, $"Hi there! I'm {_gameState.Pet.Name}, your virtual {_gameState.Pet.Species}. I'm so excited to spend time with you! How are you doing today?");
        chatHistory.AddMessage(AuthorRole.User, "I will feed you now.");
        chatHistory.AddMessage(AuthorRole.Assistant, "Yay! Thank you for feeding me! I feel so much better now. What shall we do next?");
        //chatHistory.AddMessage(AuthorRole.User, "Hello");
        // chatHistory.AddMessage(AuthorRole.Assistant, "Hello. How may I help you today?");

        _session = new(executor, this.chatHistory);

        return Task.CompletedTask;
    }

    public async Task<GameLogEntry> RunModel(ChatIdEnum chatId, string userInput, bool isStateful = true)
    {
        if (isStateful)
        {
            return await RunModel(chatId, userInput);
        }
        else
        {
            return await RunModelStateless(chatId, userInput);
        }
    }

    public async Task<GameLogEntry> RunModelStateless(ChatIdEnum chatId, string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            throw new ArgumentException("Prompt cannot be empty.", nameof(userInput));
        if (this._statelessExecutor == null)
            throw new Exception("AI Model not initialized. Call InitModel() first.");

        var userPrompt = userInput;// + $"  Your mood is {_gameState.Pet.Happiness} out of 100 your health is {_gameState.Pet.Health} out of 100 and your Hunger is {_gameState.Pet.Hunger} out of 100";

        // Collect the assistant's full response
        string response = "";
        letmespeak = true;
        await foreach (var text in _statelessExecutor.InferAsync(userInput, _inferenceParams))
        {
            response += text;  // build up response
            Debug.WriteLine(text); // optional: live output
        }
        letmespeak = false;
        response = response.Replace("User:", "");
        response = response.Replace("Assistant:", "");
        var aiResponse = new GameLogEntry
        {
            ChatId = chatId,
            SystemPrompt = _systemPrompt,
            UserPrompt = userPrompt,
            Message = response
        };

        this._gameLog.Add(aiResponse);

        return aiResponse;
    }

    public async Task<GameLogEntry> RunModel(ChatIdEnum chatId, string userInput)
    {
        if (this._session == null)
            throw new Exception("AI Model not initialized. Call InitModel() first.");
        if (string.IsNullOrWhiteSpace(userInput))
            throw new ArgumentException("Prompt cannot be empty.", nameof(userInput));

        if (letmespeak)
            return new GameLogEntry
            {
                ChatId = chatId,
                Message = "Wait for me to finish my last response!",
            };

        var userPrompt = userInput;// + $"  Your mood is {_gameState.Pet.Happiness} out of 100 your health is {_gameState.Pet.Health} out of 100 and your Hunger is {_gameState.Pet.Hunger} out of 100";

        // Collect the assistant's full response
        string response = "";
        letmespeak = true;
        await foreach (var text in this._session.ChatAsync(new ChatHistory.Message(AuthorRole.User, userPrompt), this._inferenceParams))
        {
            response += text;  // build up response
            Debug.WriteLine(text); // optional: live output
        }
        letmespeak = false;
        response = response.Replace("User:", "");
        response = response.Replace("Assistant:", "");
        var aiResponse = new GameLogEntry
        {
            ChatId = chatId,
            SystemPrompt = _systemPrompt,
            UserPrompt = userPrompt,
            Message = response
        };

        this._gameLog.Add(aiResponse);

        return aiResponse;
    }
}
