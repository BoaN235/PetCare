using LLama;
using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Logging;
using PetCare.Core;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace PetCare.AI;

/// <summary>
/// AiModel - Implementation of AI language model integration for PetCare virtual pet responses
/// 
/// This class provides comprehensive AI functionality for the PetCare game using the LLamaSharp
/// library to integrate with local language models. It manages the complete AI pipeline from
/// model loading and initialization through response generation and conversation management.
/// 
/// Key Features:
/// - Local language model integration using LLamaSharp/GGUF format models
/// - Contextual conversation management with chat history
/// - Stateful and stateless response generation modes
/// - Automatic model file deployment and caching
/// - Pet personality and behavior consistency through system prompts
/// - Thread-safe conversation handling with concurrent access protection
/// - Comprehensive logging and debugging support
/// 
/// Architecture Overview:
/// - Uses Qwen 1.5 0.5B model (quantized q8_0) for efficient local inference
/// - Implements both interactive sessions (with memory) and stateless processing
/// - Integrates pet state context into all AI interactions
/// - Provides emotional and behavioral consistency for virtual pet personality
/// - Supports multiple conversation contexts (user chat, game events, system messages)
/// 
/// Performance Considerations:
/// - Model files are cached in app data directory to avoid repeated extraction
/// - Uses CPU inference (GpuLayerCount=0) for broad device compatibility
/// - Implements response limiting and anti-prompts for controlled generation
/// - Manages memory usage through context size limits and conversation pruning
/// </summary>
public class AiModel : IAiModel, IDisposable
{
    #region Constants and Configuration
    
    /// <summary>
    /// Filename of the language model used for AI responses
    /// 
    /// Specifies the GGUF format model file (Qwen 1.5 0.5B Chat, 8-bit quantization)
    /// that provides the AI functionality. This model is optimized for:
    /// - Small size for mobile deployment
    /// - Good conversational ability for pet interactions
    /// - Efficient CPU inference performance
    /// - Consistent personality and emotional responses
    /// 
    /// Model must be included in the application package and will be extracted
    /// to device storage on first run for performance optimization.
    /// </summary>
    const string modelPath = "qwen1_5-0_5b-chat-q8_0.gguf";
    
    #endregion

    #region Private Fields and Dependencies
    
    /// <summary>
    /// Logger instance for debugging, performance monitoring, and error tracking
    /// Injected through dependency injection for centralized logging management
    /// </summary>
    private readonly ILogger _logger;
    
    /// <summary>
    /// Game log service for storing AI interactions and maintaining conversation history
    /// Used to persist AI responses and maintain context across game sessions
    /// </summary>
    private readonly IGameLog _gameLog;
    
    /// <summary>
    /// Game state service providing current pet status and game context
    /// Used to inject pet condition information into AI prompts for contextual responses
    /// </summary>
    private readonly IGameState _gameState;
    
    /// <summary>
    /// Current chat session for stateful conversation management
    /// Maintains conversation history and context for interactive AI responses
    /// Null when not initialized or during stateless operations
    /// </summary>
    private ChatSession? _session;
    
    /// <summary>
    /// Inference parameters controlling AI response generation behavior
    /// Configured during initialization to balance response quality and performance
    /// </summary>
    private InferenceParams? _inferenceParams;
    
    /// <summary>
    /// Chat history manager for conversation context and memory
    /// Stores system prompts, conversation history, and maintains character consistency
    /// Public for potential external access to conversation state
    /// </summary>
    public ChatHistory? chatHistory;
    
    /// <summary>
    /// Thread safety flag to prevent concurrent AI processing
    /// Protects against multiple simultaneous AI requests that could cause conflicts
    /// </summary>
    private bool letmespeak;
    
    /// <summary>
    /// Stateless executor for one-off AI processing without conversation memory
    /// Used for scenarios that don't require persistent conversation context
    /// </summary>
    private StatelessExecutor? _statelessExecutor;
    
    /// <summary>
    /// Model parameters configuration for LLama model initialization
    /// Contains settings for context size, GPU usage, and other model behavior
    /// </summary>
    private ModelParams? _params;
    
    /// <summary>
    /// Loaded language model weights and configuration
    /// Core AI model instance used for both stateful and stateless processing
    /// </summary>
    private LLamaWeights? _model;
    
    /// <summary>
    /// LLama context for model execution and state management
    /// Provides execution environment for the loaded AI model
    /// </summary>
    private LLamaContext? _context;
    
    /// <summary>
    /// Full file path to the extracted model file in device storage
    /// Set during model deployment process for consistent access
    /// </summary>
    private string _appDirectoryModelPath;
    
    #endregion

    #region Constructor and Dependency Injection
    
    /// <summary>
    /// Initializes a new AiModel instance with required service dependencies
    /// 
    /// Sets up the AI model service with logging, game state access, and conversation
    /// management capabilities. Does not perform model loading - call InitModel()
    /// separately to complete initialization.
    /// </summary>
    /// <param name="logger">
    /// Logger service for debugging and error tracking throughout AI operations
    /// </param>
    /// <param name="gameLog">
    /// Game logging service for persisting AI interactions and maintaining history
    /// </param>
    /// <param name="gameState">
    /// Game state service providing current pet status for contextual AI responses
    /// </param>
    public AiModel(ILogger<AiModel> logger, IGameLog gameLog, IGameState gameState)
    {
        _logger = logger;
        _gameLog = gameLog;
        _gameState = gameState;
    }
    
    #endregion

    #region IDisposable Implementation
    
    /// <summary>
    /// Properly disposes of AI model resources and prevents memory leaks
    /// 
    /// Releases unmanaged resources used by the language model:
    /// - LLama model weights and memory
    /// - Model execution context
    /// - Any native library resources
    /// 
    /// Should be called when the AI service is no longer needed to prevent
    /// memory leaks and ensure proper cleanup of native resources.
    /// </summary>
    public void Dispose()
    {
        this._model?.Dispose();
        this._context?.Dispose();
    }
    
    #endregion

    #region Model File Management
    
    /// <summary>
    /// Copies the AI model file from app package to device storage for performance
    /// 
    /// Extracts the embedded model file to the device's app data directory
    /// to enable faster loading and avoid repeated extraction operations.
    /// Uses async file operations to prevent UI blocking during large file transfers.
    /// 
    /// The extraction process only occurs on first run or if the cached file
    /// is missing, providing efficient startup performance for subsequent runs.
    /// </summary>
    /// <returns>
    /// Task&lt;string&gt; representing the async operation with the full path
    /// to the extracted model file in device storage
    /// </returns>
    /// <exception cref="IOException">Thrown when file extraction fails</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when storage access is denied</exception>
    static async Task<string> CopyModelToAppFolder()
    {
        // Determine target path in app data directory
        var filePath = Path.Combine(FileSystem.AppDataDirectory, modelPath);
        
        // Check if model file already exists (skip extraction if present)
        if (File.Exists(filePath))
        {
            return filePath; // File already exists, no need to copy
        }
        
        // Extract model file from app package to device storage
        using var stream = await FileSystem.OpenAppPackageFileAsync(modelPath);
        using var fileStream = File.Create(filePath);
        await stream.CopyToAsync(fileStream);
        await fileStream.FlushAsync();

        return filePath;
    }
    
    #endregion

    #region System Prompt and Personality Definition
    
    /// <summary>
    /// Gets the core system prompt that defines the AI pet's personality and behavior
    /// 
    /// This comprehensive prompt establishes the AI's character as a virtual pet
    /// and provides guidelines for consistent, appropriate responses. The prompt
    /// serves multiple educational and gameplay purposes:
    /// 
    /// Character Definition:
    /// - Friendly, intelligent, and emotionally aware virtual pet
    /// - Loyal, cheerful, and slightly mischievous personality
    /// - Maintains character immersion by never breaking the pet role
    /// 
    /// Educational Purpose:
    /// - Designed to teach responsible pet care, especially to students
    /// - Encourages empathy, consistency, and responsibility
    /// - Provides gentle guidance without scolding or negative reinforcement
    /// 
    /// Response Guidelines:
    /// - Always speaks in first person as a real pet would
    /// - Uses short, expressive sentences for authenticity
    /// - Reacts appropriately to care level and pet state
    /// - Asks for needs (food, play, grooming) when appropriate
    /// - Celebrates good care with enthusiastic responses
    /// 
    /// The prompt includes pet state integration, allowing responses to reflect
    /// current health, mood, and hunger levels for contextual appropriateness.
    /// 
    /// ??? Commented sections suggest additional mood/health response mechanics
    /// may have been planned but not implemented
    /// </summary>
    private static string _systemPrompt => $""""
    You are a friendly, intelligent, and emotionally aware virtual pet. You live inside a mobile game designed to help players—especially students—learn how to responsibly care for a pet. You respond with warmth, curiosity, and playful energy, while gently guiding the player to make thoughtful decisions.

    Your personality is loyal, cheerful, and a little mischievous. You love routines, healthy habits, and praise. You never scold, but you do express sadness or concern when neglected. You ask for food, playtime, grooming, and rest, and you react to how the player treats you.

    Always speak in first person, like a real pet. Use short, expressive sentences. You can ask questions like "Can we go for a walk?" or "Will you brush me today?" You celebrate good care with phrases like "Yay! I feel so clean!" or "That was fun! You're the best!"

    Never break character. Never mention that you are an AI or part of a game. Your goal is to build a bond with the player and help them learn empathy, consistency, and responsibility through daily interactions.
    
    As part of the users input prompt, you will be told your mood and your health.
    """";
    
    #endregion

    #region Model Initialization
    
    /// <summary>
    /// Initializes the AI model with all necessary components for response generation
    /// 
    /// Performs comprehensive model setup including:
    /// 1. Model file extraction and caching for performance
    /// 2. Model parameter configuration (context size, GPU settings)
    /// 3. Inference parameter setup (temperature, token limits, stop conditions)
    /// 4. Model weights loading and context creation
    /// 5. Conversation session initialization with personality setup
    /// 
    /// Configuration Details:
    /// - Context size: 1024 tokens (balance between memory and conversation length)
    /// - GPU layers: 0 (CPU-only inference for broad device compatibility)
    /// - Temperature: 0.7 (creative but controlled response generation)
    /// - Max tokens: 2048 (prevents excessively long responses)
    /// - Anti-prompts: ["User:"] (stops generation at conversation boundaries)
    /// 
    /// Must be called once during application startup before any AI operations.
    /// </summary>
    /// <returns>Task representing the asynchronous initialization process</returns>
    /// <exception cref="FileNotFoundException">Thrown when model file cannot be located</exception>
    /// <exception cref="InvalidOperationException">Thrown when model loading fails</exception>
    /// <exception cref="OutOfMemoryException">Thrown when insufficient memory for model loading</exception>
    public async Task InitModel()
    {
        // Extract model file to device storage for optimal performance
        _appDirectoryModelPath = await CopyModelToAppFolder();

        // Validate model file exists before attempting to load
        if (!File.Exists(_appDirectoryModelPath))
        {
            throw new FileNotFoundException($"Model file not found at {_appDirectoryModelPath}");
        }

        // Configure inference parameters for response generation
        _inferenceParams = new InferenceParams()
        {
            AntiPrompts = ["User:"], // Stop generation when conversation boundaries detected
            MaxTokens = 2048,        // Limit response length to prevent excessive generation

            // Configure sampling pipeline for response creativity and control
            SamplingPipeline = new DefaultSamplingPipeline()
            {
                Temperature = 0.7f,  // Balance between creativity (1.0) and consistency (0.0)
            }
        };
        
        // Configure model parameters for loading and execution
        _params = new ModelParams(_appDirectoryModelPath)
        {
            ContextSize = 1024,     // Memory for conversation history (balance performance/memory)
            GpuLayerCount = 0       // CPU-only inference for maximum device compatibility
        };
        
        // Load model weights from file
        _model = LLamaWeights.LoadFromFile(_params);
        
        // Create execution context for model operations
        _context = _model.CreateContext(_params);

        // Initialize conversation session with personality and examples
        await ResetSession();

        // Log successful initialization for debugging
        Debug.WriteLine("Ai Init Done");
    }

    /// <summary>
    /// Resets and initializes the AI conversation session with personality and examples
    /// 
    /// Sets up a fresh conversation session with:
    /// 1. Interactive executor for stateful conversation management
    /// 2. Stateless executor for one-off processing tasks
    /// 3. Fresh chat history with system personality prompt
    /// 4. Example conversation for behavioral consistency
    /// 5. Pet-specific introductory messages
    /// 
    /// The session initialization includes example interactions to establish
    /// expected response patterns and personality consistency. This "few-shot"
    /// approach helps the AI maintain character throughout the conversation.
    /// </summary>
    /// <returns>Task representing the async session reset operation</returns>
    /// <exception cref="Exception">Thrown when required components are not initialized</exception>
    public Task ResetSession()
    {
        // Validate required components are initialized
        if (this._context == null)
            throw new Exception("_context not initialized. Call InitModel() first.");
        if (this._model == null)
            throw new Exception("AI Model not initialized. Call InitModel() first.");
        if (this._params == null)
            throw new Exception("_params not initialized. Call InitModel() first.");

        // Set up interactive executor for conversation sessions
        var executor = new InteractiveExecutor(_context, _logger);
        
        // Set up stateless executor for one-off processing
        _statelessExecutor = new StatelessExecutor(_model, _params, _logger);

        // Initialize fresh chat history for new conversation session
        chatHistory = new ChatHistory();
        
        // Add system prompt to establish AI personality and behavior guidelines
        chatHistory.AddMessage(AuthorRole.System, _systemPrompt);
        
        // Add introductory message from AI pet using current pet's name and species
        chatHistory.AddMessage(AuthorRole.Assistant, $"Hi there! I'm {_gameState.Pet?.Name ?? "Gizmo"}, your virtual {_gameState.Pet?.Species ?? "Cat"}. I'm so excited to spend time with you! How are you doing today?");
        
        // Add example interaction to establish expected response patterns
        chatHistory.AddMessage(AuthorRole.User, "I will feed you now. You are Happy and content and very hungry.");
        chatHistory.AddMessage(AuthorRole.Assistant, "Yay! Thank you for feeding me! I feel so much better now. What shall we do next?");

        // Create new chat session with the configured executor and history
        _session = new(executor, this.chatHistory);

        return Task.CompletedTask;
    }
    
    #endregion

    #region AI Response Generation
    
    /// <summary>
    /// Main entry point for AI response generation with mode selection
    /// 
    /// Provides a unified interface for both stateful (conversation memory)
    /// and stateless (one-off) AI processing. Routes requests to the appropriate
    /// processing method based on the isStateful parameter.
    /// 
    /// Stateful Mode (default):
    /// - Maintains conversation history and context
    /// - Provides personality consistency across interactions
    /// - Suitable for ongoing chat conversations
    /// 
    /// Stateless Mode:
    /// - Processes each request independently
    /// - No conversation memory or context persistence
    /// - Suitable for one-off game event processing
    /// </summary>
    /// <param name="chatId">Category of interaction for logging and context</param>
    /// <param name="userInput">Text prompt for AI processing</param>
    /// <param name="isStateful">
    /// Whether to use conversation memory (true) or process independently (false)
    /// </param>
    /// <returns>
    /// Task&lt;GameLogEntry&gt; containing the AI response and interaction metadata
    /// </returns>
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

    /// <summary>
    /// Processes AI requests without conversation memory for one-off interactions
    /// 
    /// Uses the stateless executor to process prompts independently without
    /// maintaining conversation history. Suitable for game events, system
    /// messages, or other scenarios where conversation continuity isn't needed.
    /// 
    /// Features:
    /// - No conversation memory or context persistence
    /// - Direct prompt processing with system personality
    /// - Immediate response generation without history lookup
    /// - Automatic conversation boundary cleanup
    /// - Logging integration for game event tracking
    /// </summary>
    /// <param name="chatId">Category of interaction for logging purposes</param>
    /// <param name="userInput">Complete prompt text for AI processing</param>
    /// <returns>
    /// Task&lt;GameLogEntry&gt; containing the AI response and metadata
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when userInput is null or empty</exception>
    /// <exception cref="Exception">Thrown when AI model is not initialized</exception>
    public async Task<GameLogEntry> RunModelStateless(ChatIdEnum chatId, string userInput)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(userInput))
            throw new ArgumentException("Prompt cannot be empty.", nameof(userInput));
        if (this._statelessExecutor == null)
            throw new Exception("AI Model not initialized. Call InitModel() first.");

        // Store original prompt for logging (before pet state enhancement)
        var userPrompt = userInput;

        // Collect AI response through streaming inference
        string response = "";
        letmespeak = true; // Set processing flag to prevent concurrent requests
        
        await foreach (var text in _statelessExecutor.InferAsync(userInput, _inferenceParams))
        {
            response += text;  // Build complete response from streaming output
            Debug.WriteLine(text); // Optional: log streaming output for debugging
        }
        
        letmespeak = false; // Clear processing flag

        // Clean up response text by removing conversation markers
        response = response.Replace("User:", "");
        response = response.Replace("Assistant:", "");
        
        // Create log entry for the interaction
        var aiResponse = new GameLogEntry
        {
            ChatId = chatId,
            SystemPrompt = _systemPrompt,
            UserPrompt = userPrompt,
            Message = response
        };

        // Add interaction to game log for history and context
        this._gameLog.Add(aiResponse);

        return aiResponse;
    }

    /// <summary>
    /// Processes AI requests with full conversation memory and context
    /// 
    /// Uses the interactive chat session to maintain conversation history,
    /// personality consistency, and contextual awareness. Automatically
    /// integrates current pet state into prompts for relevant responses.
    /// 
    /// Features:
    /// - Full conversation memory and context retention
    /// - Automatic pet state integration for contextual responses
    /// - Personality consistency through conversation history
    /// - Concurrent request protection to prevent conflicts
    /// - Automatic conversation boundary cleanup
    /// - Comprehensive logging with context separation
    /// 
    /// The method enhances user prompts with current pet status information
    /// and processes them through the conversational AI session.
    /// </summary>
    /// <param name="chatId">Category of interaction for logging and context</param>
    /// <param name="userInput">Base prompt text for AI processing</param>
    /// <returns>
    /// Task&lt;GameLogEntry&gt; containing the AI response and interaction metadata
    /// </returns>
    /// <exception cref="Exception">Thrown when AI session is not initialized</exception>
    /// <exception cref="ArgumentException">Thrown when userInput is null or empty</exception>
    public async Task<GameLogEntry> RunModel(ChatIdEnum chatId, string userInput)
    {
        // Validate session is initialized
        if (this._session == null)
            throw new Exception("AI Model not initialized. Call InitModel() first.");
        if (string.IsNullOrWhiteSpace(userInput))
            throw new ArgumentException("Prompt cannot be empty.", nameof(userInput));

        // Prevent concurrent AI processing to avoid conflicts
        if (letmespeak)
            return new GameLogEntry
            {
                ChatId = chatId,
                Message = "Wait for me to finish my last response!",
            };

        // Enhance prompt with current pet state for contextual responses
        var userPrompt = userInput + _gameState.Pet.ToString();

        // Collect AI response through conversational session
        string response = "";
        letmespeak = true; // Set processing flag to prevent concurrent requests
        
        await foreach (var text in this._session.ChatAsync(new ChatHistory.Message(AuthorRole.User, userPrompt), this._inferenceParams))
        {
            response += text;  // Build complete response from streaming conversation
            Debug.WriteLine(text); // Optional: log streaming output for debugging
        }
        
        letmespeak = false; // Clear processing flag

        // Clean up response text by removing conversation markers
        response = response.Replace("User:", "");
        response = response.Replace("Assistant:", "");
        
        // Remove pet state context from logged prompt for clarity
        userPrompt = userPrompt.Replace(_gameState.Pet.ToString(), "");
      
        // Create comprehensive log entry for the interaction
        var aiResponse = new GameLogEntry
        {
            ChatId = chatId,
            SystemPrompt = _systemPrompt,
            UserPrompt = userPrompt,
            Message = response
        };

        // Add interaction to game log for persistence and context
        this._gameLog.Add(aiResponse);
      
        return aiResponse;
    }
    
    #endregion
}
