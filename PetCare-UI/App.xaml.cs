using PetCare.Core;

namespace PetCare.UI;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        Task.Run(async () =>
        {
            var backEnd = serviceProvider.GetService<IBackend>() ?? throw new Exception("Backend cannot load");
            await backEnd.Initialize();
            await backEnd.LoadGame();
        });
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        
        // Hide the status bar on all platforms
        #if ANDROID
      Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.SetWindowSoftInputModeAdjust(this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Pan);
        #endif

        return window;
    }
}