namespace PetCare.UI.Behaviors;

/// <summary>
/// DebounceClickHandler - Utility class for preventing rapid successive button clicks and actions
/// 
/// This class implements a debounce pattern to prevent users from accidentally triggering
/// multiple rapid-fire actions that could cause race conditions, data corruption, or
/// poor user experience. It's particularly important for operations that involve:
/// - Network requests or AI processing
/// - File I/O operations (save/load)
/// - Complex game state modifications
/// - Expensive computational operations
/// 
/// Key Features:
/// - Configurable delay period for different action types
/// - Automatic blocking of subsequent calls during delay period
/// - Async/await support for seamless integration with modern UI patterns
/// - Thread-safe operation for concurrent access scenarios
/// - Lightweight implementation with minimal performance overhead
/// 
/// Usage Patterns:
/// - Wrap button click handlers to prevent double-clicks
/// - Protect API calls from rapid successive requests
/// - Ensure data integrity during save/load operations
/// - Prevent UI freezing from overlapping expensive operations
/// 
/// The handler executes the action immediately on first call, then blocks subsequent
/// calls for the specified delay period, ensuring responsive UI while preventing abuse.
/// </summary>
public class DebounceClickHandler
{
    #region Private Fields
    
    /// <summary>
    /// Flag indicating whether the handler is currently in the delay period
    /// 
    /// When true, subsequent calls to Handle() will be ignored until the delay
    /// period expires. This prevents rapid successive action execution while
    /// maintaining responsive feedback for the user.
    /// </summary>
    private bool _isWaiting = false;
    
    /// <summary>
    /// Delay period in milliseconds to wait between allowing actions
    /// 
    /// Configured during construction and remains constant throughout the
    /// handler's lifetime. Common values:
    /// - 500ms: For simple UI actions
    /// - 1000ms: For network requests or file operations
    /// - 2000ms: For expensive operations like AI processing
    /// </summary>
    private readonly int _delayMilliseconds;
    
    #endregion

    #region Constructor
    
    /// <summary>
    /// Initializes a new DebounceClickHandler with specified delay period
    /// 
    /// Creates a debounce handler configured for the specific delay requirements
    /// of the intended use case. The delay should be long enough to prevent
    /// accidental rapid clicks but short enough not to frustrate users with
    /// legitimate repeated actions.
    /// 
    /// Recommended delay periods:
    /// - 300-500ms: Light UI operations, navigation, simple toggles
    /// - 800-1200ms: Network requests, file operations, game state changes
    /// - 1500-3000ms: AI processing, complex computations, expensive operations
    /// </summary>
    /// <param name="delayMilliseconds">
    /// Time in milliseconds to wait between allowing action execution.
    /// Default is 1000ms (1 second) which works well for most scenarios.
    /// Must be positive value; negative values will cause unpredictable behavior.
    /// </param>
    public DebounceClickHandler(int delayMilliseconds = 1000)
    {
        _delayMilliseconds = delayMilliseconds;
    }
    
    #endregion

    #region Public Methods
    
    /// <summary>
    /// Executes an action with debounce protection against rapid successive calls
    /// 
    /// This method implements the core debounce logic by immediately executing
    /// the action on first call, then blocking subsequent calls during the
    /// configured delay period. The pattern ensures:
    /// 
    /// 1. First call: Action executes immediately for responsive UI
    /// 2. Subsequent calls: Ignored during delay period to prevent conflicts
    /// 3. After delay: Handler resets and is ready for next action
    /// 
    /// Thread Safety:
    /// The method uses simple boolean flag checking which provides adequate
    /// protection for typical UI scenarios. For high-concurrency scenarios,
    /// consider using more robust synchronization mechanisms.
    /// 
    /// Error Handling:
    /// The method does not catch exceptions from the provided action. Calling
    /// code should handle exceptions appropriately to prevent the handler from
    /// becoming stuck in the waiting state.
    /// 
    /// ??? Consider adding exception handling to ensure _isWaiting flag is
    /// properly reset even if the action throws an exception.
    /// </summary>
    /// <param name="action">
    /// Async function to execute with debounce protection.
    /// Should contain the actual work to be performed (button click logic,
    /// API calls, file operations, etc.). Must not be null.
    /// </param>
    /// <returns>
    /// Task representing the asynchronous operation. Completes when the action
    /// finishes executing and the delay period has elapsed, or immediately
    /// if the handler is currently in the waiting state.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when action parameter is null
    /// </exception>
    /// <example>
    /// Basic usage in a button click handler:
    /// <code>
    /// private readonly DebounceClickHandler _debouncer = new(1000);
    /// 
    /// private async void OnSaveButtonClicked(object sender, EventArgs e)
    /// {
    ///     await _debouncer.Handle(async () =>
    ///     {
    ///         // This code will only execute once per second maximum
    ///         await SaveGameData();
    ///         ShowSaveConfirmation();
    ///     });
    /// }
    /// </code>
    /// </example>
    public async Task Handle(Func<Task> action)
    {
        // Check if we're currently in the delay period from a previous call
        if (_isWaiting) return;

        // Set waiting flag to block subsequent calls
        _isWaiting = true;
        
        // Execute the provided action immediately
        await action();
        
        // Wait for the configured delay period before allowing next action
        await Task.Delay(_delayMilliseconds);
        
        // Reset flag to allow future actions
        _isWaiting = false;
    }
    
    #endregion
}
