using PetCare.AI;
using PetCare.Core;
using System.Diagnostics;
using System.Text;

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
        new PlayerAction("Feed the pet", "I just fed you your favorite food", -5, 5, 25, -20),
        new PlayerAction("Play with the pet", "I just played your favorite game with you", -5, 15, -10, 0),
        new PlayerAction("Going to work", "I went to work today", -5, -20, -5, 50),
        new PlayerAction("Take the pet to the vet", "I took you too the vet", 35, -25, -5, -40),
        new PlayerAction("Clean the pet's space", "I cleaned your living space", 10, 5, -10, -5),
        new PlayerAction("Teach the pet a trick", "I trained you to do a new trick", 5, 15, -10, -5),
        new PlayerAction("Take a walk together", "We went on a walk", 10, 20, -15, -5),
        new PlayerAction("Give the pet a bath", "I bathed you", 15, 10, -5, -10),
        new PlayerAction("Visit the pet park", "We Played at the park", 10, 25, -20, -15),
        new PlayerAction("Buy a toy", "I got you a really cool new toy", 0, 20, -5, -25)
    ];

    public IGameState GameState { get; init; }
    public IAiModel AiModel { get; init; }
    public IGameLog GameLog { get; init; }

    public async Task Initialize()
    {
        await LoadGame();

        Debug.Print("Initing model");
        await AiModel.InitModel();
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

        GameState.Day = 1;

        await SaveGame();

        return true;
    }

    public void CreatePet(string name, string type)
    {
        GameState.Pet = new Pet(name, type);
    }

    public bool ExitGame()
    {
        return true;
    }

    public async void RunAi(ChatIdEnum chatIdEnum, string prompt, string aiprompt)
    {
        prompt = prompt + GameState.Pet.ToString();
        
        Debug.Write("Running AI");
        
        await Task.Run(async () =>
        {
            var result = await AiModel.RunModel(chatIdEnum, prompt, aiprompt);
            
            Debug.Write("Done Running AI");
        });
    }

    public async Task PerformAction(PlayerAction action)
    {
        GameState.Pet.StatsChange(action);
        
        Debug.Write("Running AI for " + action.AiPrompt);
        
        RunAi(ChatIdEnum.GameMessageLog, action.Text, action.AiPrompt);
    }
}
