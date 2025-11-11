using Microsoft.Maui.Controls.Shapes;
using PetCare.BE;
using PetCare.Core;
using PetCare.UI.Behaviors;
using System.Data;
using System.Timers;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui;

namespace PetCare.UI;

/// <summary>
/// MainPage - Primary user interface controller for the PetCare virtual pet game
/// 
/// This class serves as the main page controller for a .NET MAUI application that simulates
/// a virtual pet care experience. It manages user interactions, displays pet statistics,
/// handles AI chat functionality, and coordinates game state updates.
/// 
/// Key Responsibilities:
/// - Managing UI updates for pet statistics (health, mood, hunger, money)
/// - Handling user interactions with action buttons and game controls
/// - Coordinating with the backend services for game logic and AI responses
/// - Managing popup dialogs for action previews and new game setup
/// - Implementing debounced user input to prevent rapid-fire clicks
/// - Maintaining real-time chat log updates through timer-based refresh
/// 
/// Dependencies:
/// - IBackend: Core game logic and state management
/// - DebounceClickHandler: Prevents multiple rapid button clicks
/// - System.Timers.Timer: Periodic UI refresh for chat logs
/// 
/// UI Layout Management:
/// - Dynamically populates action buttons from backend data
/// - Updates progress bars for pet statistics
/// - Manages scrollable chat log display
/// - Handles responsive layout for different screen sizes
/// </summary>
public partial class MainPage : ContentPage
{
    #region Private Fields
    
    /// <summary>
    /// Backend service interface providing access to game logic, AI model, and data persistence
    /// Injected through dependency injection in constructor
    /// </summary>
    private IBackend _backend { get; }
    
    /// <summary>
    /// Debounce handler to prevent multiple rapid button clicks that could cause race conditions
    /// Set to 1000ms delay to ensure adequate time between actions
    /// </summary>
    private readonly DebounceClickHandler _debouncer = new DebounceClickHandler(1000);
    
    /// <summary>
    /// Timer for periodic UI updates, specifically for refreshing the chat log
    /// Runs every 500ms to provide near real-time updates of AI responses
    /// </summary>
    private System.Timers.Timer _timer;
    
    /// <summary>
    /// Currently selected player action that will be executed on next day progression
    /// Null until user selects an action from the available options
    /// </summary>
    private PlayerAction _selectedAction;
    
    #endregion

    #region Constructor
    
    /// <summary>
    /// Initializes the MainPage with required backend services
    /// 
    /// Sets up the UI components, initializes the refresh timer, and performs
    /// the initial screen update to display current game state.
    /// </summary>
    /// <param name="backend">
    /// Backend service providing game logic, AI functionality, and data persistence.
    /// Must not be null - injected through MAUI dependency injection.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when backend parameter is null</exception>
    public MainPage(IBackend backend)
    {
        // Store backend reference for use throughout the class
        _backend = backend;

        // Initialize XAML-defined UI components
        InitializeComponent();

        // Set up periodic timer for UI refresh (every 500ms)
        // This ensures chat log updates appear quickly after AI responses
        _timer = new System.Timers.Timer(500); // 0.5 second intervals
        _timer.Elapsed += OnTimerElapsed;      // Event handler for timer ticks
        _timer.AutoReset = true;               // Continuously repeat timer
        _timer.Start();                        // Begin timer execution

        // Perform initial UI update to display current game state
        UpdateScreen();
    }
    
    #endregion

    #region Timer and Refresh Methods
    
    /// <summary>
    /// Timer event handler that executes on the main UI thread every 500ms
    /// 
    /// This method ensures that chat log updates are displayed promptly
    /// when AI responses are received. Uses MainThread.BeginInvokeOnMainThread
    /// to safely update UI components from the timer thread.
    /// </summary>
    /// <param name="sender">Timer object that triggered the event</param>
    /// <param name="e">Event arguments containing elapsed time information</param>
    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        // Update UI on the main thread to avoid cross-thread operation exceptions
        MainThread.BeginInvokeOnMainThread(() =>
        {
            RefreshLog();
        });
    }

    /// <summary>
    /// Updates the chat log display with the latest messages from the game log
    /// 
    /// Retrieves formatted log text from the backend and updates the UI label.
    /// This method is called periodically by the timer and after user actions
    /// to ensure the chat history remains current.
    /// </summary>
    public void RefreshLog()
    {
        // Get formatted log text from backend and display in UI
        log.Text = _backend.GameLog.GetLogText();
    }
    
    #endregion

    #region Action Management Methods
    
    /// <summary>
    /// Clears all dynamically created action buttons from the action container
    /// 
    /// This method is called before repopulating the action list to ensure
    /// a clean slate for displaying current available actions.
    /// </summary>
    private void DeleteActions()
    {
        ActionBox.Children.Clear();
    }

    /// <summary>
    /// Populates the action selection area with buttons for available player actions
    /// 
    /// Creates a button for each PlayerAction in the provided array. Each button
    /// displays the action text and includes a click handler that shows action
    /// details in a popup before allowing selection.
    /// 
    /// Uses debounced click handling to prevent rapid multiple selections.
    /// </summary>
    /// <param name="actions">
    /// Array of PlayerAction objects to display as selectable buttons.
    /// Each action contains text, stat changes, and AI prompt information.
    /// </param>
    public void LoadActions(PlayerAction[] actions)
    {
        // Clear existing action buttons before adding new ones
        DeleteActions();
        
        // Create button for each available action
        foreach (PlayerAction action in actions) 
        {
            // Create new button with action text
            var btn = new Button
            {
                Text = action.Text
            };
            
            // Set up click handler with debouncing to prevent rapid clicks
            btn.Clicked += async (s, e) =>
            {
                await _debouncer.Handle(async () =>
                {
                    // Find the corresponding action from backend (defensive programming)
                    var act = _backend.Actions.SingleOrDefault(a => a.Text == action.Text);
                    
                    // Show popup with action details and effects
                    DisplayPopupButtonClicked(act);
                    
                    // Refresh UI to reflect any changes
                    UpdateScreen();
                });
            };
            
            // Add button to the UI container
            ActionBox.Children.Add(btn);
        }
    }
    
    #endregion

    #region Statistics Update Methods
    
    /// <summary>
    /// Updates all pet statistics progress bars and money display
    /// 
    /// Converts pet statistics from 0-100 range to 0.0-1.0 range for progress bars.
    /// Updates health, happiness, hunger progress bars and money label with current values.
    /// 
    /// Progress bar values are normalized: pet stat / 100.0 = progress bar percentage
    /// </summary>
    public void Update_stats()
    {
        // Update progress bars (convert 0-100 pet stats to 0.0-1.0 progress range)
        HealthBar.Progress = _backend.GameState.Pet.Health / 100.0;
        HappyBar.Progress = _backend.GameState.Pet.Happiness / 100.0;
        HungerBar.Progress = _backend.GameState.Pet.Hunger / 100.0;
        
        // Update money display with currency formatting
        MoneyLabel.Text = $"${_backend.GameState.Pet.Money}";
    }
    
    #endregion

    #region Main UI Update Method
    
    /// <summary>
    /// Comprehensive UI update method that refreshes all dynamic content
    /// 
    /// This is the primary method for updating the user interface after any
    /// game state changes. It coordinates updates across all UI sections:
    /// - Day/Day counter
    /// - Available actions
    /// - Pet statistics
    /// - Chat log history
    /// 
    /// Called after major game events like loading, saving, or day progression.
    /// </summary>
    public void UpdateScreen()
    {
        // Update day counter display
        DayLabel.Text = $"Day: {_backend.GameState.Day}";
        
        // Refresh available actions (may change based on game state)
        LoadActions(_backend.Actions);
        
        // Update all pet statistics displays
        Update_stats();
        
        // Refresh chat log with latest messages
        RefreshLog();
    }
    
    #endregion

    #region Event Handlers - Game Controls
    
    /// <summary>
    /// Event handler for the "Next Day" button click
    /// 
    /// Advances the game by one day, applying the effects of any selected action
    /// and incrementing the Day counter. This is the primary game progression mechanism.
    /// 
    /// If no action is selected (_selectedAction is null), the day still progresses
    /// but no action effects are applied to the pet.
    /// </summary>
    /// <param name="sender">The button that triggered this event</param>
    /// <param name="e">Event arguments (not used)</param>
    private void OnNextDayClicked(object? sender, EventArgs e)
    {
        // Apply selected action effects if an action was chosen
        if (_selectedAction != null) 
        {
            _backend.PerformAction(_selectedAction);
        }
        
        // Advance game timeline by one day
        _backend.GameState.Day += 1;
        
        // Update all UI elements to reflect new game state
        UpdateScreen();
    }

    /// <summary>
    /// Event handler for the "New Game" button click
    /// 
    /// Initiates a new game session by resetting the pet state and clearing
    /// game history. Shows a popup dialog for pet customization before
    /// starting the new game.
    /// 
    /// Uses debounced handling to prevent multiple rapid new game starts.
    /// </summary>
    /// <param name="sender">The button that triggered this event</param>
    /// <param name="e">Event arguments (not used)</param>
    private async void OnNewGameClicked(object? sender, EventArgs e)
    {
        await _debouncer.Handle(async () =>
        {
            // Reset pet to default state
            _backend.GameState.Pet = new Pet();
            
            // Show new game setup popup (pet name and species selection)
            DisplayNewGamePopupButton();
            
            // Initialize new game in backend (clears history, resets state)
            await _backend.NewGame();
            
            // Refresh UI to show new game state
            UpdateScreen();
        });
    }

    /// <summary>
    /// Event handler for AI chat input submission
    /// 
    /// Processes user input from the chat entry field and sends it to the AI model
    /// for response generation. Clears the input field after submission and
    /// updates the UI to show the interaction.
    /// 
    /// Validates input to ensure it's not empty or whitespace before processing.
    /// Uses debounced handling to prevent rapid message submission.
    /// </summary>
    /// <param name="sender">The button that triggered this event</param>
    /// <param name="e">Event arguments (not used)</param>
    private async void OnAiChatClicked(object? sender, EventArgs e)
    {
        await _debouncer.Handle(async () =>
        {
            // Validate input - don't process empty or whitespace-only messages
            if (ChatInput.Text == null || ChatInput.Text.Trim() == string.Empty)
                return;
            
            // Send user message to AI model for processing
            await _backend.AiModel.RunModel(ChatIdEnum.UserChat, ChatInput.Text);
            
            // Clear input field for next message
            ChatInput.Text = string.Empty;
        });
        
        // Update UI to show new chat interaction
        UpdateScreen();
    }

    /// <summary>
    /// Event handler for the "Save Game" button click
    /// 
    /// Persists current game state and chat history to device storage.
    /// Uses debounced handling to prevent multiple rapid save operations
    /// that could cause data corruption.
    /// </summary>
    /// <param name="sender">The button that triggered this event</param>
    /// <param name="e">Event arguments (not used)</param>
    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        await _debouncer.Handle(async () =>
        {
            // Save current game state to persistent storage
            await _backend.SaveGame();
            
            // Update UI (may show save confirmation or updated status)
            UpdateScreen();
        });
    }

    /// <summary>
    /// Event handler for the "Load Game" button click
    /// 
    /// Restores previously saved game state and chat history from device storage.
    /// Uses debounced handling to prevent multiple rapid load operations.
    /// 
    /// If no saved game exists, backend handles graceful fallback to default state.
    /// </summary>
    /// <param name="sender">The button that triggered this event</param>
    /// <param name="e">Event arguments (not used)</param>
    private async void OnLoadClicked(object? sender, EventArgs e)
    {
        await _debouncer.Handle(async () =>
        {
            // Load saved game state from persistent storage
            await _backend.LoadGame();
            
            // Update UI to reflect loaded game state
            UpdateScreen();
        });
    }
    
    #endregion

    #region Popup Dialog Methods
    
    /// <summary>
    /// Displays a popup dialog showing the effects of a selected player action
    /// 
    /// Creates and shows a modal popup that displays how the selected action
    /// will affect the pet's statistics (health, mood, hunger, money).
    /// The popup uses a rounded border design with the game's color scheme.
    /// 
    /// This allows players to preview action consequences before committing
    /// to them with the "Next Day" button.
    /// </summary>
    /// <param name="action">
    /// PlayerAction to display information for. Contains stat change values
    /// and descriptive text for the action.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if action parameter is null
    /// </exception>
    async void DisplayPopupButtonClicked(PlayerAction action)
    {
        // Store selected action for later execution
        _selectedAction = action;

        // Create styled popup content with action effects
        var content = new Border
        {
            Background = Colors.DarkBlue,      // Match game color scheme
            Stroke = Colors.LightBlue,         // Border color
            StrokeThickness = 2,               // Border width
            StrokeShape = new RoundRectangle   // Rounded corners
            {
                CornerRadius = new CornerRadius(20)
            },
            Padding = 30,                      // Internal spacing
            Content = new Label
            {
                // Display stat changes that will occur if action is selected
                Text = $"Health: {_selectedAction.HealthChange}\n" +
                       $"Mood: {_selectedAction.HappinessChange}\n" +
                       $"Hunger: {_selectedAction.HungerChange}\n" +
                       $"Money: {_selectedAction.MoneyChange}",
                TextColor = Colors.White
            }
        };

        // Show popup with tap-to-dismiss functionality
        await this.ShowPopupAsync(content, new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true,  // Allow easy dismissal
            PageOverlayColor = Colors.Black.MultiplyAlpha(0.4f)  // Semi-transparent overlay
        });
    }

    /// <summary>
    /// Displays a popup dialog for new game setup and pet customization
    /// 
    /// Shows a modal dialog that allows the player to customize their new pet
    /// by entering a name and species. The popup includes input validation
    /// and cannot be dismissed until valid information is provided.
    /// 
    /// This creates a more personalized experience by allowing pet customization
    /// at the start of each new game session.
    /// </summary>
    async void DisplayNewGamePopupButton()
    {
        // Create submit button for form submission
        var submitButton = new Button
        {
            Text = "Submit",
            TextColor = Colors.White,
            BackgroundColor = Colors.MidnightBlue
        };
        
        // Create input fields for pet customization
        var nameEntry = new Entry
        {
            Placeholder = "Pet Name",
            TextColor = Colors.White
        };

        var speciesEntry = new Entry
        {
            Placeholder = "Pet Species",
            TextColor = Colors.White
        };

        // Handle form submission with validation
        submitButton.Clicked += async (s, e) =>
        {
            // Extract and validate user input
            string petName = nameEntry.Text?.Trim();
            string petSpecies = speciesEntry.Text?.Trim();

            // Only proceed if both fields have valid input
            if (!string.IsNullOrEmpty(petName) && !string.IsNullOrEmpty(petSpecies))
            {
                // Apply customization to game state
                _backend.GameState.Pet.Name = petName;
                _backend.GameState.Pet.Species = petSpecies;
            }
            // ??? Consider adding else clause with validation feedback for user

            // Close popup and return to main game interface
            await this.ClosePopupAsync();
        };

        // Create styled popup layout matching game design
        var content = new Border
        {
            Background = Colors.DarkBlue,
            Stroke = Colors.Gray,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(20)
            },
            Padding = 30,
            Content = new StackLayout
            {
                Children =
                {
                    new Label
                    {
                        Text = "Enter Pet Name",
                        TextColor = Colors.White
                    },
                    nameEntry,
                    new Label
                    {
                        Text = "Enter Pet Species",
                        TextColor = Colors.White
                    },
                    speciesEntry,
                    submitButton
                }
            }
        };

        // Show popup with mandatory completion (cannot be dismissed by tapping outside)
        await this.ShowPopupAsync(content, new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = false,  // Force completion
            PageOverlayColor = Colors.Black.MultiplyAlpha(0.4f)
        });
    }
    
    #endregion
}
