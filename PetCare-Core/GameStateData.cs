using System.Security.Cryptography.X509Certificates;

namespace PetCare.Core;

public interface IGameStateData
{
    int Day { get; set; }
    Pet Pet { get; set; }
}

public class GameStateData : IGameStateData
{
    public int Day { get; set; }

    public Pet Pet { get; set; }
}

