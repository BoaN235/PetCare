namespace PetCare.Core;

/// <summary>
/// GameLogEntry - Data model representing a single interaction or event in the PetCare game log
/// 
/// This class encapsulates all information related to a single logged interaction between
/// the player, the AI pet, or the game system. It serves as the fundamental unit of the
/// conversation and event tracking system, preserving context and metadata necessary
/// for AI continuity and game state management.
/// 
/// Key Features:
/// - Comprehensive metadata preservation for AI context maintenance
/// - Automatic timestamping for chronological ordering and analysis
/// - Support for multiple interaction types through chat categorization
/// - Complete conversation context storage (prompts, responses, system context)
/// - Serializable structure for persistent storage and data exchange
/// 
/// Usage Scenarios:
/// - User chat messages and AI pet responses
/// - Game event logging and system notifications
/// - Player action confirmations and stat change records
/// - Debug information and error message tracking
/// - Conversation history for AI context and personality consistency
/// 
/// The class supports both simple text-based logging and complex structured interactions
/// with full context preservation for sophisticated AI conversation management.
/// </summary>
public class GameLogEntry
{
    #region Constructors
    
    /// <summary>
    /// Initializes a new GameLogEntry with default empty values
    /// 
    /// Creates an empty log entry with automatic timestamp generation.
    /// All text fields are initialized to empty strings and the chat ID
    /// is set to its default value. Primarily used for serialization
    /// and programmatic entry construction.
    /// </summary>
    public GameLogEntry() { }

    /// <summary>
    /// Initializes a new GameLogEntry with message content and user context
    /// 
    /// Creates a log entry with basic message information and associated
    /// user prompt or action. Automatically generates timestamp and
    /// initializes other fields to defaults.
    /// 
    /// This constructor is commonly used for simple logging scenarios
    /// where full AI context isn't required but user action tracking is needed.
    /// </summary>
    /// <param name="Line">
    /// Primary message content (e.g., AI response, system notification, error message)
    /// </param>
    /// <param name="userprompt">
    /// Associated user prompt or action that triggered this log entry
    /// (e.g., user's chat message, button click description, game action)
    /// </param>
    public GameLogEntry(string Line, string userprompt)
    {
        Message = Line;
        UserPrompt = userprompt;
    }
    
    #endregion

    #region Metadata Properties
    
    /// <summary>
    /// Gets or sets the category of this log entry for organization and filtering
    /// 
    /// Categorizes the log entry to enable filtering, context management, and
    /// appropriate processing by different game systems. Common categories include:
    /// - UserChat: Direct player-to-pet conversations
    /// - GameMessageLog: Game event responses and action confirmations  
    /// - System: System notifications, errors, and administrative messages
    /// 
    /// This categorization is essential for AI context management, allowing the
    /// system to provide appropriate responses based on interaction type.
    /// </summary>
    /// <value>Chat category enumeration value</value>
    public ChatIdEnum ChatId { get; set; }

    /// <summary>
    /// Gets the timestamp when this log entry was created
    /// 
    /// Automatically set to the current date and time when the log entry is
    /// instantiated. Used for chronological ordering, conversation flow analysis,
    /// and debugging time-based issues. The timestamp is immutable once set
    /// to maintain data integrity.
    /// 
    /// Essential for maintaining conversation continuity and providing context
    /// for AI responses based on when interactions occurred.
    /// </summary>
    /// <value>DateTime of log entry creation, automatically set on instantiation</value>
    public DateTime MessageTime { get; } = DateTime.Now;
    
    #endregion

    #region AI Context Properties
    
    /// <summary>
    /// Gets or sets the system prompt used by the AI for this interaction
    /// 
    /// Contains the foundational prompt that establishes the AI's personality,
    /// behavior guidelines, and response context for this specific interaction.
    /// This typically includes:
    /// - Pet personality and character traits
    /// - Response style and tone guidelines
    /// - Educational objectives and behavioral constraints
    /// - Context-specific instructions for the interaction type
    /// 
    /// Preserving the system prompt is crucial for understanding AI behavior,
    /// debugging response issues, and maintaining consistency across conversations.
    /// </summary>
    /// <value>System prompt text, empty string by default</value>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's input prompt or action that initiated this log entry
    /// 
    /// Records what the user did or said that resulted in this log entry being created:
    /// - Chat messages typed by the user
    /// - Descriptions of game actions taken (e.g., "fed the pet", "went to work")
    /// - System events triggered by user actions
    /// - Button clicks or UI interactions
    /// 
    /// This context is essential for AI response generation and conversation
    /// continuity, allowing the system to understand what the user intended
    /// and respond appropriately.
    /// </summary>
    /// <value>User prompt or action description, empty string by default</value>
    public string UserPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary message content of this log entry
    /// 
    /// Contains the main content of the log entry, which varies by entry type:
    /// - AI-generated responses to user actions or chat messages
    /// - System notifications and status updates
    /// - Error messages and diagnostic information
    /// - Game event descriptions and confirmations
    /// 
    /// This is the primary content displayed to users in the chat interface
    /// and represents the "output" of whatever process generated this log entry.
    /// </summary>
    /// <value>Primary message content, empty string by default</value>
    public string Message { get; set; } = string.Empty;
    
    #endregion
}
