using PetCare.Core;

namespace PetCare.BE;

public class GameState : BaseDataObject<GameStateData>, IGameState
{
    public GameState() : base(typeof(GameState).Name + ".json")
    {
    }

    public int Day { get => DataObj.Day; set => DataObj.Day = value; }

    public string PastActionPrompt { get => DataObj.PastActionPrompt; set => DataObj.PastActionPrompt = value; }

    public string PastStatusPrompt { get => DataObj.PastStatusPrompt; set => DataObj.PastStatusPrompt = value; }

    public Pet Pet { get => DataObj.Pet; set => DataObj.Pet = value; }
}


