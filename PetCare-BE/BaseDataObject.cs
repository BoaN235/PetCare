using System.Diagnostics;
using System.Text.Json;

namespace PetCare.BE;

/// <summary>
/// BaseDataObject - Abstract base class providing JSON persistence functionality for data models
/// 
/// This generic base class implements a standardized approach to data persistence across the
/// PetCare application, providing automatic JSON serialization/deserialization capabilities
/// with robust error handling and cross-platform file system operations.
/// 
/// Key Features:
/// - Generic type safety for strongly-typed data objects
/// - Automatic JSON serialization with human-readable formatting
/// - Cross-platform file system operations using MAUI FileSystem APIs
/// - Comprehensive error handling with debug logging
/// - Standardized file naming and location management
/// - Thread-safe async operations for non-blocking I/O
/// 
/// Architecture Design:
/// - Abstract base class following Template Method pattern
/// - Generic constraint ensures type safety and parameterless constructor availability
/// - Virtual methods allow derived classes to customize behavior while maintaining consistency
/// - Centralized file path management for predictable data storage locations
/// - Consistent error handling and logging across all data operations
/// 
/// Usage Pattern:
/// Classes inheriting from this base should focus on their specific data logic while
/// gaining automatic persistence capabilities. The base class handles all file system
/// operations, serialization concerns, and error recovery scenarios.
/// 
/// Thread Safety:
/// While individual operations are async-safe, concurrent access to the same data file
/// from multiple instances should be managed by the calling code to prevent data corruption.
/// </summary>
/// <typeparam name="T">
/// The data type to be persisted. Must be a reference type with a parameterless constructor
/// to support JSON deserialization. Should be serializable to JSON without circular references.
/// </typeparam>
public abstract class BaseDataObject<T> where T : class, new()
{
    #region Private Fields
    
    /// <summary>
    /// Full file system path where the data object will be persisted
    /// 
    /// Constructed during initialization by combining the MAUI app data directory
    /// with the provided filename. This ensures data is stored in the appropriate
    /// platform-specific location that survives app updates and provides adequate
    /// storage permissions.
    /// 
    /// The path is readonly after construction to prevent accidental modification
    /// that could lead to data loss or inconsistent storage locations.
    /// </summary>
    private readonly string _systemFilePath;
    
    #endregion

    #region Constructor
    
    /// <summary>
    /// Initializes a new BaseDataObject with the specified filename for persistence
    /// 
    /// Sets up the complete file system path for data storage by combining the
    /// platform-appropriate app data directory with the provided filename.
    /// The resulting path ensures:
    /// - Cross-platform compatibility through MAUI FileSystem APIs
    /// - Appropriate permissions for read/write operations
    /// - Data persistence across app launches and updates
    /// - Isolation from other applications and user data
    /// 
    /// The data object is initialized with a new instance of type T, providing
    /// a clean starting state for derived classes.
    /// </summary>
    /// <param name="filePath">
    /// Filename (with optional relative path) for the persisted data file.
    /// Should include appropriate file extension (typically .json).
    /// Must be a valid filename for the target platforms.
    /// Examples: "gamestate.json", "logs/chat_history.json"
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when filePath is null, empty, or contains invalid characters
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when the app data directory is not accessible (rare platform issue)
    /// </exception>
    public BaseDataObject(string filePath)
    {
        this._systemFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, filePath);
    }
    
    #endregion

    #region Protected Properties
    
    /// <summary>
    /// Gets or sets the data object instance that will be persisted
    /// 
    /// This property provides derived classes with direct access to the data
    /// object for manipulation while maintaining encapsulation. The data object
    /// is automatically initialized with a new instance of type T and can be
    /// replaced entirely if needed.
    /// 
    /// Derived classes should use this property to:
    /// - Access and modify the underlying data
    /// - Implement business logic operations
    /// - Provide typed access through wrapper properties
    /// 
    /// The property is protected to prevent external classes from bypassing
    /// the derived class's intended interface while allowing full access
    /// for implementation purposes.
    /// </summary>
    /// <value>
    /// Current instance of type T containing the persistable data.
    /// Never null - automatically initialized with new T() if not set explicitly.
    /// </value>
    protected T DataObj { get; set; } = new T();
    
    #endregion

    #region Data Import Operations
    
    /// <summary>
    /// Loads the data object from persistent storage with comprehensive error handling
    /// 
    /// Attempts to restore the data object from the file system using the following process:
    /// 1. Checks if the data file exists at the configured path
    /// 2. Reads the complete file content as a UTF-8 encoded JSON string
    /// 3. Deserializes the JSON into a strongly-typed object instance
    /// 4. Handles missing files gracefully by initializing with default values
    /// 5. Provides detailed logging for debugging and monitoring purposes
    /// 
    /// Error Recovery:
    /// - Missing file: Creates new default instance (normal for first run)
    /// - Corrupted JSON: Logs error and retains current data object state
    /// - File access issues: Logs error and continues with current state
    /// - Deserialization failures: Falls back to new instance to prevent null references
    /// 
    /// The method is virtual to allow derived classes to customize loading behavior
    /// while maintaining the core persistence logic and error handling patterns.
    /// </summary>
    /// <returns>
    /// Task representing the asynchronous import operation. The task completes
    /// when the data has been loaded or when error recovery has been completed.
    /// </returns>
    /// <exception cref="JsonException">
    /// Caught internally and logged - does not propagate to caller
    /// </exception>
    /// <exception cref="IOException">
    /// Caught internally and logged - does not propagate to caller
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// Caught internally and logged - does not propagate to caller
    /// </exception>
    public virtual async Task Import()
    {
        try
        {
            // Check if data file exists before attempting to read
            if (!File.Exists(_systemFilePath))
            {
                // Missing file is normal for first application run - not an error condition
                Debug.WriteLine($"⚠️ No existing {this.GetType().Name} file found, starting fresh.");
                DataObj = new();
                return;
            }

            // Read complete file content as JSON string
            string json = await File.ReadAllTextAsync(_systemFilePath);
            
            // Deserialize JSON to strongly-typed object with null-safety fallback
            DataObj = JsonSerializer.Deserialize<T>(json) ?? new();

            // Log successful operation for debugging and monitoring
            Debug.WriteLine($"✅ Loaded {this.GetType().Name} entries.");
        }
        catch (Exception ex)
        {
            // Comprehensive error handling - log but don't crash application
            // Maintains current DataObj state for graceful degradation
            Debug.WriteLine($"❌ Error loading {this.GetType().Name}: {ex.Message}");
        }
    }
    
    #endregion

    #region Data Export Operations
    
    /// <summary>
    /// Saves the current data object to persistent storage with formatted JSON output
    /// 
    /// Serializes the current data object to JSON format and writes it to the file system
    /// using the following process:
    /// 1. Serializes the data object to JSON with human-readable formatting
    /// 2. Writes the complete JSON string to the configured file path
    /// 3. Ensures UTF-8 encoding for cross-platform compatibility
    /// 4. Provides detailed logging for operation tracking and debugging
    /// 
    /// JSON Configuration:
    /// - WriteIndented: true - Creates human-readable JSON with proper formatting
    /// - Default serialization options for maximum compatibility
    /// - Handles complex object graphs and collections automatically
    /// 
    /// Error Handling:
    /// - Serialization failures: Logged but operation fails gracefully
    /// - File system errors: Logged with detailed error information
    /// - Permission issues: Logged and reported for troubleshooting
    /// 
    /// The method is virtual to allow derived classes to customize serialization
    /// behavior, add encryption, or implement specialized formatting requirements.
    /// </summary>
    /// <returns>
    /// Task representing the asynchronous export operation. The task completes
    /// when the data has been successfully written to storage or when error
    /// handling has been completed.
    /// </returns>
    /// <exception cref="JsonException">
    /// Caught internally and logged - may indicate circular references or unsupported types
    /// </exception>
    /// <exception cref="IOException">
    /// Caught internally and logged - indicates file system access issues
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// Caught internally and logged - indicates insufficient permissions
    /// </exception>
    public virtual async Task Export()
    {
        try
        {
            // Serialize data object to formatted JSON string
            string json = JsonSerializer.Serialize(DataObj, new JsonSerializerOptions
            {
                WriteIndented = true  // Human-readable formatting for debugging and manual inspection
            });

            // Write JSON string to file system with UTF-8 encoding
            await File.WriteAllTextAsync(_systemFilePath, json);
            
            // Log successful operation with file path for verification
            Debug.WriteLine($"✅ {this.GetType().Name} saved to: {_systemFilePath}");
        }
        catch (Exception ex)
        {
            // Comprehensive error logging for troubleshooting data persistence issues
            Debug.WriteLine($"❌ Error saving {this.GetType().Name}: {ex.Message}");
        }
    }
    
    #endregion

    #region Data Management Operations
    
    /// <summary>
    /// Clears all data and resets to default state with immediate persistence
    /// 
    /// Performs a complete data reset operation through the following steps:
    /// 1. Removes the existing data file from the file system if it exists
    /// 2. Resets the data object to a new default instance
    /// 3. Immediately saves the clean state to establish a new baseline
    /// 4. Ensures data consistency between memory and persistent storage
    /// 
    /// Use Cases:
    /// - New game initialization that requires clean state
    /// - Data corruption recovery by resetting to known good state
    /// - User-requested data reset functionality
    /// - Application reset or factory restore operations
    /// 
    /// Safety Considerations:
    /// - Operation is irreversible - all previous data will be permanently lost
    /// - Immediate export ensures new clean state is persisted
    /// - File deletion is conditional to handle cases where file doesn't exist
    /// 
    /// The method is virtual to allow derived classes to implement specialized
    /// clearing logic, such as backing up data before deletion or performing
    /// additional cleanup operations on related resources.
    /// </summary>
    /// <returns>
    /// Task representing the asynchronous clear operation. The task completes
    /// when the data has been cleared from both memory and persistent storage.
    /// </returns>
    /// <exception cref="IOException">
    /// May be thrown during file deletion or subsequent export operation
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// May be thrown if file deletion or export operations lack sufficient permissions
    /// </exception>
    public virtual async Task Clear()
    {
        // Remove existing data file if it exists (conditional to avoid exceptions)
        if (File.Exists(_systemFilePath))
        {
            File.Delete(_systemFilePath);
        }
        
        // Reset data object to clean default state
        DataObj = new();
        
        // Immediately persist the clean state to maintain consistency
        await Export();
    }
    
    #endregion
}


