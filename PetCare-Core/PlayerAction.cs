namespace PetCare.Core;

/// <summary>
/// PlayerAction - Data model representing an action that a player can perform in the pet care game
/// 
/// This class encapsulates all information needed to define a gameplay action, including
/// its display text, AI integration prompts, and the statistical effects it will have
/// on the pet when executed. PlayerActions form the core gameplay mechanics by defining
/// what activities are available to the player and their consequences.
/// 
/// Each action represents a specific pet care activity such as:
/// - Basic care (feeding, grooming, playing)
/// - Health management (vet visits, exercise)
/// - Economic activities (working, shopping)
/// - Social interactions (training, park visits)
/// 
/// The class supports both simple text-based actions and complex actions with
/// full stat modification and AI narrative integration.
/// </summary>
public class PlayerAction
{
    #region Constructors
    
    /// <summary>
    /// Initializes a new PlayerAction with default empty values
    /// 
    /// Creates an action with no text, no AI prompt, and no stat changes.
    /// Primarily used for serialization, testing, or as a base for
    /// programmatic action construction.
    /// </summary>
    public PlayerAction() { }

    /// <summary>
    /// Initializes a new PlayerAction with only display text
    /// 
    /// Creates a simple action with descriptive text but no gameplay effects.
    /// Useful for placeholder actions or actions that only provide narrative
    /// without affecting pet statistics.
    /// </summary>
    /// <param name="text">Display text shown to the player for this action</param>
    public PlayerAction(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Initializes a new PlayerAction with complete gameplay functionality
    /// 
    /// Creates a fully-featured action with display text, AI integration,
    /// and statistical effects on the pet. This is the primary constructor
    /// used for defining the core gameplay actions in the game.
    /// </summary>
    /// <param name="text">
    /// Display text shown to players in the action selection UI.
    /// Should be concise and descriptive of the action being taken.
    /// </param>
    /// <param name="aiPrompt">
    /// Prompt text sent to the AI model when this action is performed.
    /// Should be written from the perspective of the pet owner talking to the pet,
    /// describing what action was just taken. Used to generate contextual AI responses.
    /// </param>
    /// <param name="healthchange">
    /// Amount to modify the pet's health stat when this action is performed.
    /// Positive values improve health, negative values reduce health.
    /// Typical range: -50 to +50 for balanced gameplay.
    /// </param>
    /// <param name="happinesschange">
    /// Amount to modify the pet's happiness stat when this action is performed.
    /// Positive values improve mood, negative values reduce happiness.
    /// Typical range: -50 to +50 for balanced gameplay.
    /// </param>
    /// <param name="hungerchange">
    /// Amount to modify the pet's hunger stat when this action is performed.
    /// Positive values increase fullness, negative values increase hunger.
    /// Note: Higher hunger values mean more full, lower values mean more hungry.
    /// </param>
    /// <param name="moneychange">
    /// Amount to modify the player's money when this action is performed.
    /// Positive values add money (earning), negative values subtract money (spending).
    /// Used for economic gameplay elements like working or purchasing items.
    /// </param>
    public PlayerAction(string text, string aiPrompt, int healthchange, int happinesschange, int hungerchange, int moneychange)
    {
        Text = text;
        AiPrompt = aiPrompt;
        HealthChange = healthchange;
        HappinessChange = happinesschange;
        HungerChange = hungerchange;
        MoneyChange = moneychange;
    }
    
    #endregion

    #region Display Properties
    
    /// <summary>
    /// Gets or sets the display text shown to players for this action
    /// 
    /// This text appears on the action button in the UI and should clearly
    /// communicate what the player will be doing. Should be concise but
    /// descriptive enough for players to understand the action's purpose.
    /// 
    /// Examples:
    /// - "Feed the pet"
    /// - "Take the pet to the vet"
    /// - "Play with the pet"
    /// - "Go to work"
    /// </summary>
    /// <value>User-friendly action description, empty string by default</value>
    public string Text { get; set; } = string.Empty;
    
    #endregion

    #region AI Integration Properties
    
    /// <summary>
    /// Gets or sets the prompt text sent to the AI model when this action is executed
    /// 
    /// This text is used to generate contextual AI responses that reflect the
    /// action just taken by the player. The prompt should be written from the
    /// perspective of the pet owner speaking to their pet, describing what
    /// just happened in first person.
    /// 
    /// The AI uses this prompt along with the pet's current state to generate
    /// appropriate responses that maintain narrative consistency and personality.
    /// 
    /// Examples:
    /// - "I just fed you your favorite food"
    /// - "I took you to the vet for a checkup"
    /// - "We just played your favorite game together"
    /// - "I went to work today to earn money for your care"
    /// </summary>
    /// <value>AI prompt text for response generation, empty string by default</value>
    public string AiPrompt { get; set; } = string.Empty;
    
    #endregion

    #region Statistical Effect Properties
    
    /// <summary>
    /// Gets or sets the amount this action modifies the pet's health statistic
    /// 
    /// Represents the direct impact this action has on the pet's physical well-being.
    /// - Positive values: Action improves health (vet visits, exercise, grooming)
    /// - Negative values: Action may reduce health (neglect, risky activities)
    /// - Zero: Action has no direct health impact
    /// 
    /// Health changes should be balanced to create meaningful gameplay decisions
    /// without making any single action too overpowered or punishing.
    /// </summary>
    /// <value>Health modification amount, 0 by default (no change)</value>
    public int HealthChange { get; set; } = 0;

    /// <summary>
    /// Gets or sets the amount this action modifies the pet's happiness statistic
    /// 
    /// Represents the emotional impact this action has on the pet's mood and contentment.
    /// - Positive values: Action makes pet happier (playing, treats, attention)
    /// - Negative values: Action reduces happiness (being left alone, unpleasant experiences)
    /// - Zero: Action has neutral emotional impact
    /// 
    /// Happiness changes are crucial for maintaining pet engagement and creating
    /// emotional investment in the care relationship.
    /// </summary>
    /// <value>Happiness modification amount, 0 by default (no change)</value>
    public int HappinessChange { get; set; } = 0;

    /// <summary>
    /// Gets or sets the amount this action modifies the pet's hunger statistic
    /// 
    /// Represents how this action affects the pet's food satisfaction level.
    /// - Positive values: Action increases fullness (feeding, treats)
    /// - Negative values: Action increases hunger (exercise, time passing, activities)
    /// - Zero: Action doesn't affect hunger level
    /// 
    /// Note: In the hunger system, higher values indicate more fullness/satisfaction,
    /// while lower values indicate greater hunger/need for food.
    /// </summary>
    /// <value>Hunger modification amount, 0 by default (no change)</value>
    public int HungerChange { get; set; } = 0;

    /// <summary>
    /// Gets or sets the amount this action modifies the player's money
    /// 
    /// Represents the economic impact of performing this action on the player's finances.
    /// - Positive values: Action earns money (working, selling items)
    /// - Negative values: Action costs money (buying food, vet bills, toys)
    /// - Zero: Action has no financial impact
    /// 
    /// Money changes create resource management gameplay, requiring players to
    /// balance earning income with spending on pet care needs.
    /// </summary>
    /// <value>Money modification amount, 0 by default (no change)</value>
    public int MoneyChange { get; set; } = 0;
    
    #endregion
}

