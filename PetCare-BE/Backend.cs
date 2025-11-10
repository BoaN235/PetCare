using PetCare.Core;
using System.Diagnostics;

namespace PetCare.BE;

public class Backend : IBackend
{
    public Backend(IGameState gameState, IAiModel aiModel, IGameLog gameLog)
    {
        GameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        AiModel = aiModel ?? throw new ArgumentNullException(nameof(aiModel));
        GameLog = gameLog ?? throw new ArgumentNullException(nameof(gameLog));
    }

    public PlayerAction[] Actions { get; } = [
        new PlayerAction("Feed the pet", "Feeding the Pet", -5, 5, 25, -20),
        new PlayerAction("Play with the pet", "Playing with the pet", -5, 15, -10, 0),
        new PlayerAction("Going to work", "Going to work today", -5, -20, -5, 50),
        new PlayerAction("Take the pet to the vet", "Taking the pet to the vet", 35, -25, -5, -40),
        new PlayerAction("Clean the pet's space", "Cleaning the pet's living area", 10, 5, -10, -5),
        new PlayerAction("Teach the pet a trick", "Training the pet", 5, 15, -10, -5),
        new PlayerAction("Take a walk together", "Going for a walk with the pet", 10, 20, -15, -5),
        new PlayerAction("Give the pet a bath", "Bathing the pet", 15, 10, -5, -10),
        new PlayerAction("Visit the pet park", "Playing at the pet park", 10, 25, -20, -15),
        new PlayerAction("Buy a toy", "Getting a new toy for the pet", 0, 20, -5, -25)
    ];

    public IGameState GameState { get; init; }
    public IAiModel AiModel { get; init; }
    public IGameLog GameLog { get; init; }


    public async Task Initialize()
    {
        await LoadGame();
    }

    public async Task SaveGame()
    {
        await GameLog.Export();
        await GameState.Export();
    }


    public async Task LoadGame()
    {
        await GameLog.Import();
        await GameState.Import();
    }

    public async Task<bool> NewGame()
    {
        await GameLog.Clear();
        await GameState.Clear();

        GameState.Pet = new Pet("Gizmo", "Cat");
        GameState.Week = 1;

        await SaveGame();

        return true;
    }

    public void createPet(string name, string type)
    {
        GameState.Pet = new Pet(name, type);
    }

    public bool ExitGame()
    {
        return true;
    }

    public async void RunAi(ChatIdEnum chatIdEnum, string prompt)
    {
        // Run AI processing in background
        Debug.Write("Running AI");
        await Task.Run(async () =>
        {
            var result = await AiModel.RunModel(chatIdEnum, prompt);
            GameLog.Add(result.Message);
            Debug.Write("Done Running AI");
        });

    }

    public async Task PerformAction(PlayerAction action)
    {
        // Run AI processing in background
        GameState.Week++;
        
        Debug.Write("Running AI for " + action.Text);
        await Task.Run(async () =>
        {
            var result = await AiModel.RunModel(ChatIdEnum.GameMessageLog, action.Text);
            //GameLog.Add(result.Message);
            Debug.Write("Done Running AI");
        });
        GameState.Pet.StatsChange(action);
    }
}
