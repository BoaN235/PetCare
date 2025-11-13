namespace PetCare.Core;

public interface IBackend
{
    IGameState GameState { get; }
    IAiModel AiModel { get; }
    IGameLog GameLog { get; }
    PlayerAction[] Actions { get; }
    Task PerformAction(PlayerAction action);
    Task Initialize();
    bool ExitGame();
    Task SaveGame();
    Task LoadGame();
    Task<bool> NewGame();
    void RunAi(ChatIdEnum chatIdEnum, string prompt);
}
