using Microsoft.Extensions.Logging;
using PetCare.AI;
using PetCare.BE;
using PetCare.Core;
using System.Runtime.InteropServices;
using CommunityToolkit.Maui; // Add this using if not present

namespace PetCare.UI;

/// <summary>
/// MauiProgram - Application configuration and dependency injection setup for PetCare
/// 
/// This static class configures the .NET MAUI application with all necessary services,
/// dependencies, and platform-specific settings required for the PetCare virtual pet game.
/// It serves as the central configuration point for the entire application architecture.
/// 
/// Key Responsibilities:
/// - Native library configuration for AI model integration (LLamaSharp)
/// - Dependency injection container setup with service lifetimes
/// - Logging configuration for debugging and monitoring
/// - MAUI framework and community toolkit integration
/// - Font registration and theme configuration
/// - Cross-platform compatibility settings
/// 
/// Architecture Overview:
/// - Implements singleton pattern for core services to maintain state consistency
/// - Configures native library resolution for cross-platform AI model support
/// - Sets up comprehensive logging infrastructure for development and production
/// - Integrates MAUI Community Toolkit for enhanced UI capabilities
/// - Enforces light theme for consistent visual experience
/// 
/// The configuration ensures proper service lifetimes, dependency resolution,
/// and platform-specific optimizations for optimal performance across all supported devices.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the MAUI application with all required services and settings
    /// 
    /// This method performs comprehensive application setup including:
    /// 1. Native library resolution for AI model integration
    /// 2. MAUI framework and community toolkit configuration
    /// 3. Font registration for consistent typography
    /// 4. Dependency injection container setup with proper service lifetimes
    /// 5. Logging infrastructure configuration for debugging
    /// 6. Application theme enforcement for visual consistency
    /// 
    /// Service Registration:
    /// - All core services registered as singletons for state consistency
    /// - Services injected in dependency order to satisfy requirements
    /// - Interface-based registration for testability and modularity
    /// 
    /// Platform Compatibility:
    /// - Native library resolution configured for cross-platform AI support
    /// - CPU-only inference settings for maximum device compatibility
    /// - Theme settings configured for consistent appearance
    /// </summary>
    /// <returns>
    /// Configured MauiApp instance ready for platform-specific launching
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when service configuration fails or dependencies cannot be resolved
    /// </exception>
    public static MauiApp CreateMauiApp()
    {
        #region Native Library Configuration for AI Integration
        
        // Configure native library resolution for LLamaSharp AI integration
        // This setup ensures that the AI model can load native libraries across platforms
        // Essential for cross-platform compatibility of the language model functionality
        NativeLibrary.SetDllImportResolver(typeof(LLama.Native.NativeApi).Assembly, (libraryName, assembly, searchPath) =>
        {
            // Handle LLama native library resolution for different platform naming conventions
            if (libraryName == "llama")
            {
                IntPtr handle;
                
                // Try Linux/Android naming convention first
                if (NativeLibrary.TryLoad("libllama.so", out handle))
                {
                    return handle;
                }
                
                // Fallback to generic naming convention
                if (NativeLibrary.TryLoad("llama", out handle))
                {
                    return handle;
                }
            }
            
            // Return null to continue with default resolution if custom resolution fails
            return IntPtr.Zero;
        });
        
        #endregion

        #region MAUI Application Builder Configuration
        
        // Create MAUI application builder for configuration
        var builder = MauiApp.CreateBuilder();

        // Configure core MAUI application with community toolkit integration
        builder
            .UseMauiApp<App>()                    // Register main application class
            .UseMauiCommunityToolkit()            // Enable community toolkit features (popups, behaviors, etc.)
            .ConfigureFonts(fonts =>              // Register application fonts
            {
                // Register default font files for consistent typography across platforms
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Conthrax-SemiBold.otf", "ContraxFont");
            });
        
        #endregion

        #region Logging Configuration
        
        // Configure comprehensive logging system for debugging and monitoring
        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();              // Remove default providers for clean configuration
            logging.AddDebug();                    // Add debug output provider for development
            logging.SetMinimumLevel(LogLevel.Trace); // Enable all log levels for comprehensive debugging
        });
        
        #endregion

        #region Dependency Injection Service Registration
        
        // Register core game services as singletons to maintain state consistency
        // Singleton lifetime ensures the same instance is used throughout the application
        // This is critical for maintaining game state, conversation history, and AI context
        
        /// <summary>
        /// Game state service - manages pet statistics, progression, and player data
        /// Registered as singleton to maintain consistent game state across all components
        /// </summary>
        builder.Services.AddSingleton<IGameState, GameState>();
        
        /// <summary>
        /// Game logging service - manages chat history, AI interactions, and event logging
        /// Registered as singleton to maintain conversation continuity and context
        /// </summary>
        builder.Services.AddSingleton<IGameLog, GameLog>();
        
        /// <summary>
        /// AI model service - provides language model integration and response generation
        /// Registered as singleton to maintain conversation context and avoid repeated model loading
        /// </summary>
        builder.Services.AddSingleton<IAiModel, AiModel>();
        
        /// <summary>
        /// Backend service - coordinates game logic, AI integration, and data persistence
        /// Registered as singleton to maintain consistent game flow and state management
        /// Depends on all other services and serves as the primary facade for UI components
        /// </summary>
        builder.Services.AddSingleton<IBackend, Backend>();
        
        #endregion

        #region Debug-Specific Configuration
        
        // Additional debug logging configuration for development builds
        // Provides enhanced debugging capabilities during development and testing
#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        #endregion

        #region Application Theme Configuration
        
        // Enforce light theme for consistent visual experience across platforms
        // Ensures the game maintains its intended visual design regardless of system settings
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = AppTheme.Light;
        }
        
        #endregion

        #region Application Build and Return
        
        // Build and return the configured MAUI application
        // At this point, all services are registered and the application is ready for platform-specific launching
        return builder.Build();
        
        #endregion
    }
}
