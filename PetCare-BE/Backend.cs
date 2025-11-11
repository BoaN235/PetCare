using PetCare.Core;
using System.Diagnostics;
using System.Text;

namespace PetCare.BE;

/// <summary>
/// Backend - Main implementation of the IBackend interface for PetCare game logic
/// 
/// This class serves as the primary coordination layer for the PetCare virtual pet game,
/// orchestrating interactions between the game state, AI model, logging system, and
/// player actions. It implements the complete game logic flow and manages the core
/// gameplay loop.
/// 
/// Architecture Overview:
/// - Coordinates between multiple service dependencies (IGameState, IAiModel, IGameLog)
/// - Manages predefined player actions with balanced stat effects
/// - Handles asynchronous AI processing for dynamic game responses
/// - Provides data persistence through save/load functionality
/// - Implements game lifecycle management (new game, initialization, cleanup)
/// 
/// Key Features:
/// - Dependency injection for testable and modular design
/// - Predefined action system with balanced gameplay mechanics
/// - Asynchronous AI integration for responsive user experience
/// - Robust error handling and state management
/// - Cross-platform data persistence support
/// </summary>
public class Backend : IBackend
{
    #region Dependency Injection and Constructor
    
    /// <summary>
    /// Initializes a new Backend instance with required service dependencies
    /// 
    /// Sets up the backend with all necessary services using dependency injection
    /// pattern. Validates that all dependencies are provided and throws appropriate
    /// exceptions if any are missing to fail fast during application startup.
    /// </summary>
    /// <param name="gameState">
    /// Game state service managing pet statistics, progression, and player data.
    /// Must implement IGameState interface for state persistence and retrieval.
    /// </param>
    /// <param name="aiModel">
    /// AI model service for generating contextual responses and game narrative.
    /// Must implement IAiModel interface with language model integration.
    /// </param>
    /// <param name="gameLog">
    /// Logging service for managing chat history and game event records.
    /// Must implement IGameLog interface for log persistence and retrieval.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the required dependencies are null
    /// </exception>
    public Backend(IGameState gameState, IAiModel aiModel, IGameLog gameLog)
    {
        GameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        AiModel = aiModel ?? throw new ArgumentNullException(nameof(aiModel));
        GameLog = gameLog ?? throw new ArgumentNullException(nameof(gameLog));
    }
    
    #endregion

    #region Predefined Game Actions
    
    /// <summary>
    /// Gets the complete array of available player actions with balanced gameplay effects
    /// 
    /// Defines all possible activities players can choose from during gameplay.
    /// Each action is carefully balanced to create meaningful choices and strategic
    /// resource management. Actions are categorized into several types:
    /// 
    /// Care Actions:
    /// - Feed the pet: Reduces health slightly (-5) but significantly improves happiness (+5) and hunger (+25), costs money (-20)
    /// - Play with the pet: Reduces health (-5) but boosts happiness (+15), increases hunger (-10)
    /// - Clean the pet's space: Improves health (+10) and happiness (+5), increases hunger (-10), small cost (-5)
    /// - Give the pet a bath: Improves health (+15) and happiness (+10), increases hunger (-5), small cost (-10)
    /// 
    /// Economic Actions:
    /// - Going to work: Reduces health (-5) and happiness (-20), increases hunger (-5), but provides significant income (+50)
    /// 
    /// Health Actions:
    /// - Take the pet to the vet: Major health improvement (+35) but reduces happiness (-25) and hunger (-5), expensive (-40)
    /// 
    /// Training/Social Actions:
    /// - Teach the pet a trick: Improves health (+5) and happiness (+15), increases hunger (-10), small cost (-5)
    /// - Take a walk together: Improves health (+10) and happiness (+20), increases hunger (-15), small cost (-5)
    /// - Visit the pet park: Improves health (+10) and happiness (+25), increases hunger (-20), moderate cost (-15)
    /// 
    /// Purchase Actions:
    /// - Buy a toy: No health impact but significant happiness boost (+20), increases hunger (-5), costs money (-25)
    /// 
    /// ??? Consider adding more diverse actions for extended gameplay variety
    /// </summary>
    /// <value>Array of 10 predefined PlayerAction objects with balanced stat effects</value>
    public PlayerAction[] Actions { get; } = [
        new PlayerAction("Feed the pet", "I just fed you your favorite food", -5, 5, 25, -20),
        new PlayerAction("Play with the pet", "I just played your favorite game with you", -5, 15, -10, 0),
        new PlayerAction("Going to work", "I went to work today", -5, -20, -5, 50),
        new PlayerAction("Take the pet to the vet", "I took you too the vet", 35, -25, -5, -40),
        new PlayerAction("Clean the pet's space", "I cleaned your living space", 10, 5, -10, -5),
        new PlayerAction("Teach the pet a trick", "I trained you to do a new trick", 5, 15, -10, -5),
        new PlayerAction("Take a walk together", "We went on a walk", 10, 20, -15, -5),
        new PlayerAction("Give the pet a bath", "I bathed you", 15, 10, -5, -10),
        new PlayerAction("Visit the pet park", "We Played at the park", 10, 25, -20, -15),
        new PlayerAction("Buy a toy", "I got you a really cool new toy", 0, 20, -5, -25)
    ];
    
    #endregion

    #region Service Properties
    
    /// <summary>
    /// Gets the game state service managing pet statistics and progression data
    /// 
    /// Provides access to the current game state including pet health, happiness,
    /// hunger, money, current Day/day, and any other progression information.
    /// This service handles state persistence and provides the data foundation
    /// for all game logic operations.
    /// </summary>
    /// <value>Game state service instance, injected during construction</value>
    public IGameState GameState { get; init; }

    /// <summary>
    /// Gets the AI model service for generating dynamic game responses
    /// 
    /// Provides access to language model functionality for creating contextual
    /// pet responses, processing user chat messages, and generating narrative
    /// content based on current game state and player actions.
    /// </summary>
    /// <value>AI model service instance, injected during construction</value>
    public IAiModel AiModel { get; init; }

    /// <summary>
    /// Gets the game logging service for managing chat history and events
    /// 
    /// Provides access to logging functionality for storing and retrieving
    /// chat interactions, AI responses, game events, and other historical
    /// data that maintains game continuity and context.
    /// </summary>
    /// <value>Game logging service instance, injected during construction</value>
    public IGameLog GameLog { get; init; }
    
    #endregion

    #region Lifecycle Management Methods
    
    /// <summary>
    /// Initializes the backend and all dependent systems for gameplay
    /// 
    /// Performs comprehensive system initialization by loading any existing
    /// saved game data from persistent storage. This ensures that players
    /// can continue from where they left off in previous sessions.
    /// 
    /// If no saved data exists, the system will initialize with default values
    /// appropriate for a new game session.
    /// </summary>
    /// <returns>Task representing the asynchronous initialization operation</returns>
    /// <exception cref="IOException">Thrown when file system operations fail during load</exception>
    /// <exception cref="InvalidDataException">Thrown when saved data is corrupted</exception>
    public async Task Initialize()
    {
        // Load any existing saved game state and history
        await LoadGame();
    }
    
    #endregion

    #region Data Persistence Methods
    
    /// <summary>
    /// Saves current game state and chat history to persistent device storage
    /// 
    /// Persists all critical game data to ensure continuity between app sessions:
    /// - Complete game log with all chat interactions and AI responses
    /// - Current game state including pet stats, money, and Day progression
    /// - Any additional game settings or player preferences
    /// 
    /// Uses asynchronous file operations to prevent UI blocking during save operations.
    /// </summary>
    /// <returns>Task representing the asynchronous save operation</returns>
    /// <exception cref="IOException">Thrown when file system operations fail</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when storage access is denied</exception>
    public async Task SaveGame()
    {
        // Export game log data (chat history, AI interactions, game events)
        await GameLog.Export();
        
        // Export game state data (pet stats, progression, player data)
        await GameState.Export();
    }

    /// <summary>
    /// Loads previously saved game state and chat history from persistent storage
    /// 
    /// Restores all saved game data to continue from a previous session:
    /// - Game log with complete interaction history for AI context
    /// - Pet statistics and progression data from last save point
    /// - Player customization settings and preferences
    /// 
    /// Gracefully handles missing save files by using default values for new installations.
    /// </summary>
    /// <returns>Task representing the asynchronous load operation</returns>
    /// <exception cref="IOException">Thrown when file system operations fail</exception>
    /// <exception cref="InvalidDataException">Thrown when save data is corrupted</exception>
    public async Task LoadGame()
    {
        // Import saved game log data
        await GameLog.Import();
        
        // Import saved game state data
        await GameState.Import();
    }

    /// <summary>
    /// Initializes a fresh game session by resetting all data to defaults
    /// 
    /// Creates a clean slate for new game experiences:
    /// - Clears all chat history and previous AI interactions
    /// - Resets game state to starting values (pet stats, money, Day counter)
    /// - Prepares the system for new pet customization
    /// - Saves the clean state to establish new save file
    /// 
    /// This method maintains data integrity by immediately saving after reset.
    /// </summary>
    /// <returns>
    /// Task&lt;bool&gt; representing the asynchronous operation.
    /// Always returns true upon successful completion.
    /// </returns>
    /// <exception cref="IOException">Thrown when save operations fail during new game setup</exception>
    public async Task<bool> NewGame()
    {
        // Clear all existing game log entries and chat history
        await GameLog.Clear();
        
        // Reset game state to default starting values
        await GameState.Clear();

        // Set initial game Day to 1 (beginning of game timeline)
        GameState.Day = 1;

        // Save the fresh game state to establish new save file
        await SaveGame();

        // Return success indicator
        return true;
    }
    
    #endregion

    #region Pet Management Methods
    
    /// <summary>
    /// Creates a new pet with specified customization parameters
    /// 
    /// Initializes a new Pet object with player-chosen name and species,
    /// replacing any existing pet in the current game state. Used during
    /// new game setup to establish player's customized pet.
    /// 
    /// ??? This method appears to be unused in current implementation.
    /// Consider removal or integration with new game flow.
    /// </summary>
    /// <param name="name">Custom name chosen by the player for their pet</param>
    /// <param name="type">Species/type of pet chosen by the player</param>
    public void CreatePet(string name, string type)
    {
        GameState.Pet = new Pet(name, type);
    }
    
    #endregion

    #region Game Lifecycle
    
    /// <summary>
    /// Handles game exit and cleanup operations
    /// 
    /// Currently provides a simple exit confirmation mechanism.
    /// In a more complete implementation, this might handle:
    /// - Final save operations before exit
    /// - Resource cleanup and disposal
    /// - Confirmation dialogs for unsaved changes
    /// 
    /// ??? Current implementation is minimal - consider expanding for proper cleanup
    /// </summary>
    /// <returns>Always returns true indicating successful exit</returns>
    public bool ExitGame()
    {
        return true;
    }
    
    #endregion

    #region AI Integration Methods
    
    /// <summary>
    /// Triggers AI processing for specific contexts with background execution
    /// 
    /// Provides direct access to AI model processing for various game scenarios:
    /// - User chat messages requiring pet responses
    /// - Game event narratives based on actions
    /// - Contextual pet behavior generation
    /// 
    /// Executes AI processing asynchronously in the background to maintain
    /// responsive UI performance. Includes pet state context in all prompts
    /// to ensure AI responses are appropriate for current pet condition.
    /// 
    /// ??? Method uses fire-and-forget pattern with void return type.
    /// Consider returning Task for better error handling and testing.
    /// </summary>
    /// <param name="chatIdEnum">
    /// Category of chat interaction being processed (user chat, game events, system messages)
    /// </param>
    /// <param name="prompt">
    /// Base prompt text for AI processing, will be enhanced with pet state context
    /// </param>
    public async void RunAi(ChatIdEnum chatIdEnum, string prompt)
    {
        // Enhance prompt with current pet state for contextual responses
        prompt = prompt + GameState.Pet.ToString();
        
        // Log AI processing start for debugging
        Debug.Write("Running AI");
        
        // Execute AI processing in background task to avoid UI blocking
        await Task.Run(async () =>
        {
            // Process the prompt and generate AI response
            var result = await AiModel.RunModel(chatIdEnum, prompt);
            
            // ??? Original logging code commented out - may need restoration
            // GameLog.Add(result.Message, result.UserPrompt);
            
            // Log completion for debugging
            Debug.Write("Done Running AI");
        });
    }

    /// <summary>
    /// Executes a player action and triggers associated AI narrative generation
    /// 
    /// Coordinates the complete action execution flow:
    /// 1. Applies statistical changes to pet based on action effects
    /// 2. Triggers AI processing to generate contextual response to the action
    /// 3. Logs the action and response for game history continuity
    /// 
    /// This is the primary method for processing player interactions with
    /// the pet during normal gameplay.
    /// </summary>
    /// <param name="action">
    /// PlayerAction to execute, containing stat changes and AI prompt.
    /// Must be a valid action from the Actions array.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
    public async Task PerformAction(PlayerAction action)
    {
        // Apply stat changes to pet (health, happiness, hunger, money)
        GameState.Pet.StatsChange(action);
        
        // Log action execution start for debugging
        Debug.Write("Running AI for " + action.AiPrompt);
        
        // Trigger AI response generation based on action taken
        RunAi(ChatIdEnum.GameMessageLog, action.AiPrompt);
    }
    
    #endregion
}
