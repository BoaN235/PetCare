namespace PetCare.Core;

/// <summary>
/// IGameLogData - Interface defining the contract for game log data management in PetCare
/// 
/// This interface establishes the standard operations for managing game log entries,
/// providing a unified API for logging various types of interactions and events
/// throughout the PetCare virtual pet game. It serves as the foundation for
/// conversation history, event tracking, and AI context management.
/// 
/// Key Responsibilities:
/// - Adding different types of log entries (simple messages, structured entries)
/// - Providing access to the complete log entry collection
/// - Supporting various logging scenarios from simple text to complex AI interactions
/// - Maintaining data consistency and organization for downstream consumers
/// 
/// Implementation Considerations:
/// - Should support thread-safe operations for concurrent logging
/// - Must provide efficient access to log entries for UI display and AI context
/// - Should handle different log entry types gracefully
/// - Must maintain chronological ordering of entries
/// 
/// This interface is typically implemented by classes that provide persistent
/// storage capabilities and integrate with the broader game logging ecosystem.
/// </summary>
public interface IGameLogData
{
    #region Log Entry Addition Methods
    
    /// <summary>
    /// Adds a log entry with both message content and associated user prompt
    /// 
    /// Creates a log entry that captures both the message/response content and
    /// the user action or prompt that triggered it. This method is ideal for
    /// logging interactions where the user's intent or action needs to be
    /// preserved alongside the system's response.
    /// 
    /// Common use cases:
    /// - AI responses to user chat messages
    /// - System confirmations of user actions
    /// - Game event responses triggered by player decisions
    /// </summary>
    /// <param name="message">
    /// The primary message content to log (e.g., AI response, system notification)
    /// </param>
    /// <param name="userPrompt">
    /// The user's input or action that triggered this message
    /// (e.g., chat message, button click description)
    /// </param>
    void Add(string message, string userPrompt);

    /// <summary>
    /// Adds a simple log entry with only message content
    /// 
    /// Creates a basic log entry for standalone messages that don't have
    /// an associated user prompt or trigger. Useful for system-generated
    /// notifications, autonomous game events, or informational messages.
    /// 
    /// Common use cases:
    /// - System startup and shutdown messages
    /// - Autonomous game events (time-based stat changes)
    /// - Error notifications and warnings
    /// - Debug and diagnostic information
    /// </summary>
    /// <param name="message">
    /// The message content to log (e.g., system notification, error message)
    /// </param>
    void Add(string message);

    /// <summary>
    /// Adds a complete structured log entry with full metadata
    /// 
    /// Accepts a fully-formed GameLogEntry object containing all metadata
    /// including timestamps, chat categories, system prompts, and context
    /// information. This method provides maximum flexibility and is typically
    /// used by AI systems and complex game logic that needs to preserve
    /// complete interaction context.
    /// 
    /// Common use cases:
    /// - AI model output with complete conversation context
    /// - Complex game events with multiple data points
    /// - Imported log entries from external sources
    /// - Structured logging with rich metadata
    /// </summary>
    /// <param name="entry">
    /// Complete GameLogEntry object with all metadata and content
    /// </param>
    void Add(GameLogEntry entry);
    
    #endregion

    #region Data Access Properties
    
    /// <summary>
    /// Gets the complete collection of log entries for direct access and manipulation
    /// 
    /// Provides access to all logged entries in chronological order, enabling
    /// advanced operations like filtering, searching, analytics, and bulk processing.
    /// The collection should maintain insertion order to preserve conversation
    /// flow and event sequencing.
    /// 
    /// Usage scenarios:
    /// - UI display of chat history
    /// - AI context building for conversation continuity
    /// - Analytics and reporting on user interactions
    /// - Data export and backup operations
    /// - Advanced filtering and search operations
    /// 
    /// Implementation note: Consider returning IReadOnlyList or similar to
    /// prevent external modification of the collection structure while still
    /// allowing access to individual entries.
    /// </summary>
    /// <value>
    /// List of all GameLogEntry objects in chronological order
    /// </value>
    List<GameLogEntry> Entries { get; }
    
    #endregion
}
