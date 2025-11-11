namespace PetCare.Core;

/// <summary>
/// IBackend - Core backend service interface for the PetCare virtual pet game
/// 
/// This interface defines the contract for the main backend service that coordinates
/// all game logic, AI interactions, data persistence, and game state management.
/// It serves as the primary facade for frontend components to interact with the
/// underlying game systems.
/// 
/// The backend is responsible for:
/// - Managing game state and pet statistics
/// - Coordinating AI model interactions and responses
/// - Handling game log and chat history
/// - Providing available player actions
/// - Managing save/load functionality
/// - Orchestrating game progression and action execution
/// 
/// Implementation Details:
/// - Should be registered as singleton in DI container for state consistency
/// - Must handle async operations for AI processing and file I/O
/// - Should provide thread-safe access to shared game state
/// - Must gracefully handle errors in AI processing and data persistence
/// </summary>
public interface IBackend
{
    #region Core Game Components
    
    /// <summary>
    /// Gets the current game state including pet status, Day progression, and player data
    /// 
    /// Provides access to the central game state object that contains:
    /// - Pet statistics (health, happiness, hunger, money)
    /// - Current game Day/day counter
    /// - Player progression data
    /// - Any temporary game state information
    /// 
    /// This property should always return a valid, non-null game state object.
    /// </summary>
    /// <value>Current game state instance</value>
    IGameState GameState { get; }

    /// <summary>
    /// Gets the AI model service for generating pet responses and game narrative
    /// 
    /// Provides access to the AI service that:
    /// - Generates contextual pet responses to player actions
    /// - Creates dynamic game narrative based on pet status
    /// - Handles user chat interactions with the virtual pet
    /// - Maintains conversation context and personality consistency
    /// 
    /// The AI model should be initialized before use and handle processing asynchronously.
    /// </summary>
    /// <value>AI model service instance</value>
    IAiModel AiModel { get; }

    /// <summary>
    /// Gets the game log service for managing chat history and game events
    /// 
    /// Provides access to logging functionality that:
    /// - Stores all chat interactions between player and AI pet
    /// - Maintains game event history for context
    /// - Supports different log categories (system, chat, game events)
    /// - Handles log persistence and retrieval
    /// 
    /// Used for displaying chat history and maintaining game continuity.
    /// </summary>
    /// <value>Game logging service instance</value>
    IGameLog GameLog { get; }

    /// <summary>
    /// Gets the array of available player actions that can be performed
    /// 
    /// Returns a collection of PlayerAction objects representing all possible
    /// activities the player can choose from during gameplay:
    /// - Pet care actions (feeding, playing, grooming)
    /// - Economic actions (working, shopping)
    /// - Health actions (vet visits, exercise)
    /// - Social actions (training, park visits)
    /// 
    /// Each action contains stat change information and AI prompt text.
    /// This array should remain constant during gameplay for consistency.
    /// </summary>
    /// <value>Array of available player actions</value>
    PlayerAction[] Actions { get; }
    
    #endregion

    #region Game Logic Methods
    
    /// <summary>
    /// Executes a player action and applies its effects to the game state
    /// 
    /// Processes the selected action by:
    /// 1. Applying stat changes to the pet (health, happiness, hunger, money)
    /// 2. Triggering AI response generation based on the action
    /// 3. Logging the action and response to game history
    /// 4. Updating any derived game state values
    /// 
    /// This method coordinates between multiple systems to provide a cohesive
    /// game experience when the player performs an action.
    /// </summary>
    /// <param name="action">
    /// PlayerAction to execute. Must be a valid action from the Actions array.
    /// Contains stat changes and AI prompt for response generation.
    /// </param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when backend is not initialized</exception>
    Task PerformAction(PlayerAction action);
    
    #endregion

    #region Initialization and Lifecycle
    
    /// <summary>
    /// Initializes the backend service and all dependent systems
    /// 
    /// Performs startup initialization including:
    /// - Loading existing game state from storage
    /// - Initializing AI model and loading trained weights
    /// - Setting up logging system and loading chat history
    /// - Validating all system dependencies
    /// - Preparing the game for player interaction
    /// 
    /// Should be called once during application startup before any other operations.
    /// </summary>
    /// <returns>Task representing the asynchronous initialization</returns>
    /// <exception cref="InvalidOperationException">Thrown when initialization fails</exception>
    /// <exception cref="FileNotFoundException">Thrown when required model files are missing</exception>
    Task Initialize();

    /// <summary>
    /// Exits the game and performs cleanup operations
    /// 
    /// Handles graceful game shutdown by:
    /// - Saving current game state to prevent data loss
    /// - Disposing of AI model resources
    /// - Closing file handles and network connections
    /// - Cleaning up temporary resources
    /// 
    /// Should be called before application termination.
    /// </summary>
    /// <returns>True if exit was successful, false if there were issues</returns>
    /// <exception cref="InvalidOperationException">Thrown when cleanup fails</exception>
    bool ExitGame();
    
    #endregion

    #region Data Persistence
    
    /// <summary>
    /// Saves current game state and chat history to persistent storage
    /// 
    /// Persists all game data including:
    /// - Pet statistics and progression data
    /// - Chat history and AI interaction logs
    /// - Game settings and player preferences
    /// - Current Day/day and game timeline
    /// 
    /// Uses device-specific storage mechanisms (local files, preferences, etc.)
    /// to ensure data survives application restarts.
    /// </summary>
    /// <returns>Task representing the asynchronous save operation</returns>
    /// <exception cref="IOException">Thrown when file system operations fail</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when storage access is denied</exception>
    Task SaveGame();

    /// <summary>
    /// Loads previously saved game state and chat history from persistent storage
    /// 
    /// Restores game data including:
    /// - Pet statistics and progression from last save
    /// - Previous chat interactions and AI responses
    /// - Game settings and player customizations
    /// - Timeline position and game Day
    /// 
    /// If no saved data exists, initializes with default values.
    /// Should gracefully handle corrupted or missing save files.
    /// </summary>
    /// <returns>Task representing the asynchronous load operation</returns>
    /// <exception cref="IOException">Thrown when file system operations fail</exception>
    /// <exception cref="InvalidDataException">Thrown when save data is corrupted</exception>
    Task LoadGame();

    /// <summary>
    /// Starts a new game session by resetting all game state to defaults
    /// 
    /// Initializes a fresh game by:
    /// - Resetting pet to default statistics (health, happiness, hunger)
    /// - Clearing chat history and game logs
    /// - Setting Day counter back to 1
    /// - Resetting money and progression values
    /// - Preparing for new pet customization
    /// 
    /// Typically called when player selects "New Game" option.
    /// </summary>
    /// <returns>
    /// Task&lt;bool&gt; representing the asynchronous operation.
    /// Returns true if new game was successfully created.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when new game creation fails</exception>
    Task<bool> NewGame();
    
    #endregion

    #region AI Integration
    
    /// <summary>
    /// Manually triggers AI processing for specific chat contexts and prompts
    /// 
    /// Allows direct interaction with the AI model for various purposes:
    /// - Processing user chat messages
    /// - Generating game event narratives
    /// - Creating contextual pet responses
    /// - Handling special game scenarios
    /// 
    /// This method provides lower-level access to the AI system compared to
    /// the automatic processing that occurs during PerformAction.
    /// 
    /// ??? This method signature may need review - void return type suggests
    /// fire-and-forget behavior which could lead to unhandled exceptions
    /// </summary>
    /// <param name="chatIdEnum">
    /// Category of chat/interaction being processed (user chat, game log, system messages)
    /// </param>
    /// <param name="prompt">
    /// Text prompt to send to the AI model for processing.
    /// Should include appropriate context for the interaction type.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when prompt is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when AI model is not initialized</exception>
    void RunAi(ChatIdEnum chatIdEnum, string prompt);
    
    #endregion
}
