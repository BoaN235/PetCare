using System.Xml.Linq;

namespace PetCare.Core;

public class Pet
{
    public string Name { get; set; }
    public int Age { get; set; } = 0;
    public double Health { get; set; } = 100.0;
    public double Happiness { get; set; } = 100.0;
    public double Hunger { get; set; } = 100.0;
    public int Money { get; set; } = 50;
    public string[] State { get; set; }
    public string Species { get; set; }

    public string HealthText => Health switch
    {
        >= 75.0 => "healthy",
        >= 50.0 => "okay",
        >= 25.0 => "sick",
        _ => "very sick"
    };

    public string HappinessText => Happiness switch
    {
        >= 75.0 => "happy",
        >= 50.0 => "content",
        >= 25.0 => "sad",
        _ => "depressed"
    };
    public string HungerText => Hunger switch
    {
        >= 80.0 => "full",
        >= 50.0 => "hungry",
        >= 30.0 => "very hungry",
        _ => "starving"
    };

    public Pet(string Name, string Species)
    {
    }

    public Pet() : this("gizmo", "cat")
    {
    }

    public void StatsChange(PlayerAction action) 
    { 
        Health += action.HealthChange;
        Happiness += action.HappinessChange;
        Hunger += action.HungerChange;
        Money += action.MoneyChange;
        if (Health > 100)
        {
            Health = 100;
        }
        if (Happiness > 100)
        {
            Happiness = 100;
        }
        if (Hunger > 100)
        {
            Hunger = 100;
        }
    }

    public override string ToString()
    {


        var sb = new System.Text.StringBuilder();
        //sb.Append($"You are a {Species} who is named {Name} aged {Age}.");
        //sb.Append($"Your health is at {Health}, Your happiness is at {Happiness}, and Your hunger is at {Hunger}. You are {HealthText} and {HappinessText} and {HungerText}.");
        sb.Append($"You are {HealthText} and {HappinessText} and {HungerText}.");
        return sb.ToString();

    }
}