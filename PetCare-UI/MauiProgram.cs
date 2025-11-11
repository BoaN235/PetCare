using Microsoft.Extensions.Logging;
using PetCare.AI;
using PetCare.BE;
using PetCare.Core;
using System.Runtime.InteropServices;
using CommunityToolkit.Maui; // Add this using if not present
namespace PetCare.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Set the DllImport resolver BEFORE any LLamaSharp usage
        NativeLibrary.SetDllImportResolver(typeof(LLama.Native.NativeApi).Assembly, (libraryName, assembly, searchPath) =>
        {
            if (libraryName == "llama")
            {
                IntPtr handle;
                if (NativeLibrary.TryLoad("libllama.so", out handle))
                {
                    return handle;
                }
                if (NativeLibrary.TryLoad("llama", out handle))
                {
                    return handle;
                }
            }
            return IntPtr.Zero;
        });

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() // <-- Chain this directly after .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configure logging via services
        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders(); // Optional: clear default providers
            logging.AddDebug();       // Add Debug provider
            logging.SetMinimumLevel(LogLevel.Trace); // Log everything
        });

        builder.Services.AddSingleton<IGameState, GameState>();
        builder.Services.AddSingleton<IGameLog, GameLog>();
        builder.Services.AddSingleton<IAiModel, AiModel>();
        builder.Services.AddSingleton<IBackend, Backend>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = AppTheme.Light;
        }

        return builder.Build();

    }
}
