namespace PetCare.Core;

public class PlayerAction
{

    public PlayerAction() { }
    public PlayerAction(string text)
    {
        Text = text;
    }
    public PlayerAction(string text, string aiPrompt, int healthchange, int happinesschange, int hungerchange, int moneychange)
    {
        Text = text;
        AiPrompt = aiPrompt;
        HealthChange = healthchange;
        HappinessChange = happinesschange;
        HungerChange = hungerchange;
        MoneyChange = moneychange;
    }

    public string Text { get; set; } = string.Empty;
    public string AiPrompt { get; set; } = string.Empty;

    public int HealthChange { get; set; } = 0;
    public int HappinessChange { get; set; } = 0;
    public int HungerChange { get; set; } = 0;
    public int MoneyChange { get; set; } = 0;

}