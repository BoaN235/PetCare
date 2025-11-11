using PetCare.Core;

namespace PetCare.BE;
public class GameStateData : IGameStateData
{
    public int Day { get; set; }
    public Pet Pet { get; set; } = new Pet("Gizmo", "Cat");

    public string PastActionPrompt { get; set; } = "First Action";

    public string PastStatusPrompt { get; set; } = "First Action";
}
