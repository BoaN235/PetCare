using PetCare.Core;

namespace PetCare.UI;

public partial class App : Application
{
    private IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loadingPage = new LoadingPage();
        var window = new Window(loadingPage);

        Task.Run(async () =>
        {
            try
            {
                var backEnd = _serviceProvider.GetService<IBackend>() ?? throw new Exception("Backend cannot load");
                await backEnd.Initialize();
                await backEnd.LoadGame();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    window.Page = new AppShell();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during initialization: {ex.Message}");
            }
        });

        #if ANDROID
    Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.SetWindowSoftInputModeAdjust(this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Pan);
    #endif

        return window;
    }
}