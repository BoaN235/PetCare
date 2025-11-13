using LLama;
using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Logging;
using PetCare.Core;
using System.Diagnostics;

namespace PetCare.AI;

public class AiModel : IAiModel, IDisposable
{
    const string modelPath = "qwen1_5-0_5b-chat-q8_0.gguf";
    //const string modelPath = "qwen25-3b-instruct-q6_k.gguf";

    private readonly ILogger _logger;
    private readonly IGameLog _gameLog;
    private readonly IGameState _gameState;
    private InferenceParams? _inferenceParams;
    private bool letmespeak;
    private StatelessExecutor? _statelessExecutor;
    private InteractiveExecutor? _interactiveExecutor;
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

    public void Dispose()
    {
        _model?.Dispose();
        _context?.Dispose();
    }

    static async Task<string> CopyModelToAppFolder()
    {
        var filePath = Path.Combine(FileSystem.AppDataDirectory, modelPath);
        
        if (File.Exists(filePath))
        {
            return filePath;
        }
        
        using var stream = await FileSystem.OpenAppPackageFileAsync(modelPath);
        using var fileStream = File.Create(filePath);
        await stream.CopyToAsync(fileStream);
        await fileStream.FlushAsync();

        return filePath;
    }

    private static string _systemPrompt => $""""
    You are a friendly, intelligent, and emotionally aware virtual pet. You live inside a mobile game designed to help players—especially students—learn how to responsibly care for a pet. You respond with warmth, curiosity, and playful energy, while gently guiding the player to make thoughtful decisions.

    Your personality is loyal, cheerful, and a little mischievous. You love routines, healthy habits, and praise. You never scold, but you do express sadness or concern when neglected. You ask for food, playtime, grooming, and rest, and you react to how the player treats you.

    Always speak in first person, like a real pet. Use short, expressive sentences. You can ask questions like "Can we go for a walk?" or "Will you brush me today?" You celebrate good care with phrases like "Yay! I feel so clean!" or "That was fun! You're the best!"

    Never break character. Never mention that you are an AI or part of a game. Your goal is to build a bond with the player and help them learn empathy, consistency, and responsibility through daily interactions.
    
    As part of the users input prompt, you will be told your mood and your health.
    """";
            
    public async Task InitModel()
    {
        _appDirectoryModelPath = await CopyModelToAppFolder();

        if (!File.Exists(_appDirectoryModelPath))
        {
            throw new FileNotFoundException($"Model file not found at {_appDirectoryModelPath}");
        }
        _logger.LogDebug("Model Loaded");
        _inferenceParams = new InferenceParams()
        {
            AntiPrompts = ["User:"],
            MaxTokens = 2048,
            SamplingPipeline = new DefaultSamplingPipeline()
            {
                Temperature = 0.7f,
            }
        };
        _logger.LogDebug("Interface Parms");

        _params = new ModelParams(_appDirectoryModelPath)
        {
            ContextSize = 1024,
            GpuLayerCount = 0
        };
        
        _model = LLamaWeights.LoadFromFile(_params);
        _context = _model.CreateContext(_params);

        await ResetSession();

        _logger.LogDebug("Ai Init Done");
    }

    public async Task ResetSession()
    {
        if (_context == null)
            throw new Exception("_context not initialized. Call InitModel() first.");
        if (_model == null)
            throw new Exception("AI Model not initialized. Call InitModel() first.");
        if (_params == null)
            throw new Exception("_params not initialized. Call InitModel() first.");

        _interactiveExecutor = new InteractiveExecutor(_context, _logger);
        _statelessExecutor = new StatelessExecutor(_model, _params, _logger);

        if (_gameLog.Entries.Count > 0)
            await RunModel(ChatIdEnum.UserChat, "The game was just restarted. Did you enjoy your nap?", isStateful: true);
        else
            await RunModel(ChatIdEnum.UserChat, "Are you ready to play a new game? Why dont you introduce yourself.", isStateful: true);
    }

    public async Task<GameLogEntry> RunModel(ChatIdEnum chatId, string userInput, bool isStateful = true)
    {
        try
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
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error running model");

            var aiResponse = new GameLogEntry
            {
                ChatId = chatId,
                SystemPrompt = _systemPrompt,
                UserPrompt = userInput,
                Message = "Sorry, I encountered an error while processing your request. " + ex.Message,
            };

            _gameLog.Add(aiResponse);
            return aiResponse;
        }

    }

    public async Task<GameLogEntry> RunModelStateless(ChatIdEnum chatId, string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            throw new ArgumentException("Prompt cannot be empty.", nameof(userInput));
        if (_statelessExecutor == null)
            throw new Exception("AI Model not initialized. Call InitModel() first.");

        var userPrompt = userInput;

        string response = "";
        letmespeak = true;
        
        await foreach (var text in _statelessExecutor.InferAsync(userInput, _inferenceParams))
        {
            response += text;
            Debug.WriteLine(text);
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

        _gameLog.Add(aiResponse);

        return aiResponse;
    }

    public async Task<GameLogEntry> RunModel(ChatIdEnum chatId, string userInput)
    {
        if (_interactiveExecutor == null)
            throw new Exception("AI Model not initialized. Call InitModel() first.");
        if (string.IsNullOrWhiteSpace(userInput))
            throw new ArgumentException("Prompt cannot be empty.", nameof(userInput));

        if (letmespeak)
            return new GameLogEntry
            {
                ChatId = chatId,
                Message = "Wait for me to finish my last response!",
            };

        var chatHistory = new ChatHistory();

        chatHistory.AddMessage(AuthorRole.System, _systemPrompt + "You are a virtual pet inside a mobile game. Respond to the user accordingly." + _gameState.Pet.ToString());

        chatHistory.AddMessage(AuthorRole.Assistant, $"Hi there! I'm {_gameState.Pet?.Name ?? "Gizmo"}, your virtual {_gameState.Pet?.Species ?? "Cat"}. I'm so excited to spend time with you! How are you doing today?");
        chatHistory.AddMessage(AuthorRole.User, "I will feed you now. You are Happy and content and very hungry.");
        chatHistory.AddMessage(AuthorRole.Assistant, "Yay! Thank you for feeding me! I feel so much better now. What shall we do next?");

        ChatSession session = new(_interactiveExecutor, chatHistory);

        var userPrompt = userInput;

        string response = "";
        letmespeak = true;
        
        await foreach (var text in session.ChatAsync(new ChatHistory.Message(AuthorRole.User, userPrompt), _inferenceParams))
        {
            response += text;
            Debug.WriteLine(text);
        }
        
        letmespeak = false;

        response = response.Replace("User:", "");
        response = response.Replace("Assistant:", "");
        
        if (_gameState.Pet != null)
            userPrompt = userPrompt.Replace(_gameState.Pet.ToString(), "");
      
        var aiResponse = new GameLogEntry
        {
            ChatId = chatId,
            SystemPrompt = _systemPrompt,
            UserPrompt = userPrompt,
            Message = response
        };

        _gameLog.Add(aiResponse);
      
        return aiResponse;
    }
}
