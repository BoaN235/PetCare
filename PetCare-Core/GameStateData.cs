using System.Security.Cryptography.X509Certificates;

namespace PetCare.Core;

public interface IGameStateData
{
    int Week { get; set; }
    Pet Pet { get; set; }
}

public class GameStateData : IGameStateData
{
    public int Week { get; set; }

    public Pet Pet { get; set; }
}

