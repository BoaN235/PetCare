namespace PetCare.Core;

/// <summary>
/// ChatIdEnum - Enumeration defining categories for chat interactions and log entries in PetCare
/// 
/// This enumeration provides standardized categorization for all logged interactions
/// and messages within the PetCare virtual pet game. It enables proper organization,
/// filtering, and context-appropriate processing of different interaction types.
/// 
/// The categorization is essential for:
/// - AI context management and response generation
/// - Log filtering and retrieval operations
/// - UI display organization and conversation flow
/// - Analytics and debugging support
/// - Maintaining separation between different interaction types
/// 
/// Each category serves specific purposes in the game's conversation and event
/// management system, allowing for targeted processing and appropriate responses.
/// </summary>
public enum ChatIdEnum
{
    /// <summary>
    /// Game event messages and action-based AI responses
    /// 
    /// Used for AI responses generated from player actions within the game world.
    /// This category includes:
    /// - Pet reactions to care actions (feeding, playing, grooming)
    /// - Responses to game events (vet visits, work activities, purchases)
    /// - Stat-based emotional responses reflecting pet's current state
    /// - Action confirmations and acknowledgments from the virtual pet
    /// 
    /// These messages are typically triggered by gameplay mechanics rather than
    /// direct chat input, providing narrative context for player actions and
    /// creating a more immersive pet care experience.
    /// 
    /// Example scenarios:
    /// - "Yay! Thank you for feeding me! I feel so much better now."
    /// - "I'm a bit tired after our walk, but it was fun!"
    /// - "The vet visit wasn't my favorite, but I feel healthier now."
    /// </summary>
    GameMessageLog,

    /// <summary>
    /// Direct conversational interactions between player and AI pet
    /// 
    /// Used for freeform chat conversations where the player types messages
    /// directly to their virtual pet. This category enables:
    /// - Personal conversations between player and pet
    /// - Questions about the pet's well-being or preferences
    /// - Casual interaction and relationship building
    /// - Educational conversations about pet care topics
    /// 
    /// These interactions maintain conversational context and personality
    /// consistency, allowing for natural back-and-forth communication that
    /// enhances the emotional bond between player and virtual pet.
    /// 
    /// Example scenarios:
    /// - Player: "How are you feeling today?"
    /// - Player: "What's your favorite activity?"
    /// - Player: "Are you hungry or would you prefer to play?"
    /// </summary>
    UserChat,

    /// <summary>
    /// System-generated notifications, errors, and administrative messages
    /// 
    /// Used for technical communications and game system notifications that
    /// don't come from the AI pet personality. This category includes:
    /// - Save/load operation confirmations and errors
    /// - Game initialization and startup messages
    /// - Error notifications and troubleshooting information
    /// - Administrative alerts and system status updates
    /// - Debug information and development messages
    /// 
    /// These messages maintain clear separation between system functionality
    /// and the pet's personality, ensuring that technical communications
    /// don't interfere with the immersive pet care experience.
    /// 
    /// Example scenarios:
    /// - "Game saved successfully"
    /// - "Failed to load AI model - please restart"
    /// - "Network connection required for enhanced features"
    /// </summary>
    System
}