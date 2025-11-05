using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace PetCare_UI
{
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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
