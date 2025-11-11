namespace PetCare.Core;

/// <summary>
/// IAiModel - Interface for AI language model integration in PetCare virtual pet game
/// 
/// Defines the contract for AI services that provide natural language processing
/// and response generation for virtual pet interactions. Implementations should
/// provide contextual, personality-consistent responses that enhance the pet care experience.
/// </summary>
public interface IAiModel
{
    /// <summary>
    /// Initializes the AI model and prepares it for response generation
    /// 
    /// Must be called once during application startup before any response generation.
    /// Handles model loading, configuration, and memory allocation.
    /// </summary>
    /// <returns>Task representing the asynchronous initialization process</returns>
    Task InitModel();
    
    /// <summary>
    /// Generates AI responses with configurable processing mode
    /// 
    /// Primary interface supporting both stateful (conversation memory) and 
    /// stateless (independent) processing modes.
    /// </summary>
    /// <param name="chatId">Category of interaction for logging and context</param>
    /// <param name="userInput">Text prompt to process</param>
    /// <param name="isStateful">Whether to maintain conversation memory (default: true)</param>
    /// <returns>Task containing the AI response with metadata</returns>
    Task<GameLogEntry> RunModel(ChatIdEnum chatId, string userInput, bool isStateful = true);

    /// <summary>
    /// Processes AI requests without conversation memory for one-off interactions
    /// 
    /// Optimal for game events, system messages, and scenarios that don't require
    /// conversational context or memory persistence.
    /// </summary>
    /// <param name="chatId">Category of interaction for logging</param>
    /// <param name="userInput">Complete prompt text for independent processing</param>
    /// <returns>Task containing the generated response and metadata</returns>
    Task<GameLogEntry> RunModelStateless(ChatIdEnum chatId, string userInput);

    /// <summary>
    /// Generates AI responses with full conversation context and memory
    /// 
    /// Dedicated method for conversational interactions requiring chat history,
    /// personality consistency, and contextual awareness.
    /// </summary>
    /// <param name="chatId">Category of conversational interaction</param>
    /// <param name="userinput">User's message for conversational processing</param>
    /// <returns>Task containing the conversational response with metadata</returns>
    Task<GameLogEntry> RunModel(ChatIdEnum chatId, string userinput);
}


