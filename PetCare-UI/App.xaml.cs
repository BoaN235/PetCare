using PetCare.Core;

namespace PetCare.UI;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        Task.Run(async () =>
        {
            var aiModel = serviceProvider.GetService<IAiModel>() ?? throw new Exception("Ai Model cannot load");
            await aiModel.InitModel();

            var backEnd = serviceProvider.GetService<IBackend>() ?? throw new Exception("Backend cannot load");
            await backEnd.Initialize();
        });
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}