using System.Xml.Linq;

namespace PetCare.Core;

/// <summary>
/// Pet - Core data model representing the virtual pet in the game
/// 
/// This class encapsulates all the essential attributes and behaviors of the virtual pet
/// that players care for throughout the game. It manages the pet's physical and emotional
/// state, provides human-readable status descriptions, and handles stat modifications
/// from player actions.
/// 
/// Key Features:
/// - Statistical tracking (health, happiness, hunger, money)
/// - Automatic status text generation based on current stats
/// - Stat boundary enforcement (0-100 ranges with caps)
/// - Customizable pet identity (name, species, age)
/// - String representation for AI context integration
/// 
/// The pet serves as the central focus of the game, with all player actions
/// aimed at maintaining and improving the pet's well-being across multiple dimensions.
/// </summary>
public class Pet
{
    #region Basic Identity Properties
    
    /// <summary>
    /// Gets or sets the pet's name as chosen by the player
    /// 
    /// Used throughout the game for personalization and AI interactions.
    /// The AI model references this name when generating responses to create
    /// a more personal and immersive experience for the player.
    /// </summary>
    /// <value>Pet's custom name, default is "gizmo"</value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the pet's age in game units
    /// 
    /// Represents the pet's maturity level and may influence certain game mechanics
    /// or AI responses. Currently initialized to 0 for new pets.
    /// 
    /// ??? Age progression mechanics may need implementation - unclear if this
    /// automatically increments or requires manual management
    /// </summary>
    /// <value>Pet's age, starting at 0 for new pets</value>
    public int Age { get; set; } = 0;

    /// <summary>
    /// Gets or sets the pet's species as chosen by the player
    /// 
    /// Determines the type of animal the player is caring for (cat, dog, bird, etc.).
    /// Used by the AI model to generate species-appropriate responses and behaviors.
    /// Influences the personality and response patterns of the virtual pet.
    /// </summary>
    /// <value>Pet's species type, default is "cat"</value>
    public string Species { get; set; }
    
    #endregion

    #region Core Statistics Properties
    
    /// <summary>
    /// Gets or sets the pet's physical health level (0-100 scale)
    /// 
    /// Represents the pet's overall physical well-being and fitness.
    /// - 100: Perfect health, no physical issues
    /// - 75-99: Good health, minor concerns
    /// - 50-74: Moderate health, some issues present
    /// - 25-49: Poor health, significant problems
    /// - 0-24: Critical health, requires immediate attention
    /// 
    /// Affected by actions like vet visits (+), neglect (-), exercise (+/-).
    /// Critical for game balance as low health may limit available actions.
    /// </summary>
    /// <value>Health percentage from 0.0 to 100.0</value>
    public double Health { get; set; } = 100.0;

    /// <summary>
    /// Gets or sets the pet's emotional happiness/mood level (0-100 scale)
    /// 
    /// Represents the pet's emotional state and contentment with care received.
    /// - 100: Extremely happy, loves current treatment
    /// - 75-99: Very content, enjoys interactions
    /// - 50-74: Moderately happy, neutral mood
    /// - 25-49: Somewhat sad, needs more attention
    /// - 0-24: Depressed, requires immediate emotional care
    /// 
    /// Influenced by play time (+), social activities (+), neglect (-).
    /// Directly impacts AI personality and response tone in interactions.
    /// </summary>
    /// <value>Happiness percentage from 0.0 to 100.0</value>
    public double Happiness { get; set; } = 100.0;

    /// <summary>
    /// Gets or sets the pet's hunger/satiation level (0-100 scale)
    /// 
    /// Represents how well-fed and satisfied the pet is currently.
    /// - 100: Completely full, doesn't need food
    /// - 80-99: Well-fed, satisfied
    /// - 50-79: Getting hungry, should eat soon
    /// - 30-49: Very hungry, needs food
    /// - 0-29: Starving, critical need for food
    /// 
    /// Decreases over time and with certain activities, increases with feeding.
    /// High hunger negatively impacts health and happiness if left unaddressed.
    /// </summary>
    /// <value>Hunger/fullness percentage from 0.0 to 100.0</value>
    public double Hunger { get; set; } = 100.0;

    /// <summary>
    /// Gets or sets the pet owner's available money for purchases and care
    /// 
    /// Represents the financial resources available for pet care activities.
    /// Used for:
    /// - Purchasing food, toys, and accessories
    /// - Paying for veterinary care and treatments
    /// - Accessing premium activities and services
    /// - Managing economic aspects of pet ownership
    /// 
    /// Increased through work activities, decreased through spending actions.
    /// Creates resource management gameplay as players balance earning and spending.
    /// </summary>
    /// <value>Available money amount, starting at 50 for new games</value>
    public int Money { get; set; } = 50;
    
    #endregion

    #region Additional State Properties
    
    /// <summary>
    /// Gets or sets additional state information for the pet
    /// 
    /// ??? Purpose and usage of this property is unclear from current implementation.
    /// May be intended for storing temporary states, status effects, or extended
    /// pet attributes that don't fit into the core stat model.
    /// 
    /// Requires further documentation or implementation details.
    /// </summary>
    /// <value>Array of state strings, purpose undefined</value>
    public string[] State { get; set; }
    
    #endregion

    #region Computed Status Properties
    
    /// <summary>
    /// Gets a human-readable description of the pet's current health status
    /// 
    /// Automatically converts the numeric Health value into descriptive text
    /// that's more intuitive for players to understand. Used in UI displays
    /// and AI context to provide meaningful health information.
    /// 
    /// Status Ranges:
    /// - "healthy": 75-100 health points
    /// - "okay": 50-74 health points  
    /// - "sick": 25-49 health points
    /// - "very sick": 0-24 health points
    /// </summary>
    /// <value>Descriptive health status text</value>
    public string HealthText => Health switch
    {
        >= 75.0 => "healthy",
        >= 50.0 => "okay", 
        >= 25.0 => "sick",
        _ => "very sick"
    };

    /// <summary>
    /// Gets a human-readable description of the pet's current emotional state
    /// 
    /// Converts the numeric Happiness value into descriptive text for better
    /// player understanding and AI context. Directly influences how the AI
    /// model responds to player interactions.
    /// 
    /// Status Ranges:
    /// - "happy": 75-100 happiness points
    /// - "content": 50-74 happiness points
    /// - "sad": 25-49 happiness points  
    /// - "depressed": 0-24 happiness points
    /// </summary>
    /// <value>Descriptive happiness status text</value>
    public string HappinessText => Happiness switch
    {
        >= 75.0 => "happy",
        >= 50.0 => "content",
        >= 25.0 => "sad", 
        _ => "depressed"
    };

    /// <summary>
    /// Gets a human-readable description of the pet's current hunger level
    /// 
    /// Translates the numeric Hunger value into intuitive descriptive text.
    /// Used for UI feedback and AI context to help players understand when
    /// feeding actions are needed.
    /// 
    /// Status Ranges:
    /// - "full": 80-100 hunger points (well-fed)
    /// - "hungry": 50-79 hunger points (ready to eat)
    /// - "very hungry": 30-49 hunger points (needs food soon)
    /// - "starving": 0-29 hunger points (critical need)
    /// </summary>
    /// <value>Descriptive hunger status text</value>
    public string HungerText => Hunger switch
    {
        >= 80.0 => "full",
        >= 50.0 => "hungry",
        >= 30.0 => "very hungry",
        _ => "starving"
    };
    
    #endregion

    #region Constructors
    
    /// <summary>
    /// Initializes a new Pet instance with specified name and species
    /// 
    /// Creates a pet with custom identity while using default statistical values.
    /// All stats (health, happiness, hunger) start at maximum values (100),
    /// and money starts at the default amount (50).
    /// 
    /// ??? Constructor body is currently empty - may need implementation
    /// to properly set the Name and Species properties from parameters.
    /// </summary>
    /// <param name="Name">Custom name for the pet</param>
    /// <param name="Species">Species/type of the pet</param>
    public Pet(string Name, string Species)
    {
        // ??? Implementation missing - should set this.Name = Name and this.Species = Species
    }

    /// <summary>
    /// Initializes a new Pet instance with default values
    /// 
    /// Creates a default pet named "gizmo" of species "cat" with all
    /// statistics set to their starting values. This constructor is used
    /// when creating new games or when specific customization isn't needed.
    /// </summary>
    public Pet() : this("gizmo", "cat")
    {
        // Delegates to parameterized constructor with default values
    }
    
    #endregion

    #region Stat Modification Methods
    
    /// <summary>
    /// Applies the effects of a player action to the pet's statistics
    /// 
    /// Modifies the pet's stats based on the action taken by the player.
    /// Automatically enforces upper bounds (100 max) for each stat to prevent
    /// values from exceeding realistic ranges. Does not enforce lower bounds,
    /// allowing stats to go negative if actions have severe consequences.
    /// 
    /// Changes Applied:
    /// - Health: Modified by action.HealthChange
    /// - Happiness: Modified by action.HappinessChange  
    /// - Hunger: Modified by action.HungerChange
    /// - Money: Modified by action.MoneyChange
    /// 
    /// ??? Lower bounds enforcement may be needed to prevent negative stats
    /// which could cause display issues or game logic problems.
    /// </summary>
    /// <param name="action">
    /// PlayerAction containing the stat changes to apply.
    /// Each change value can be positive (improvement) or negative (deterioration).
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
    public void StatsChange(PlayerAction action) 
    { 
        // Apply stat changes from the action
        Health += action.HealthChange;
        Happiness += action.HappinessChange;
        Hunger += action.HungerChange;
        Money += action.MoneyChange;
        
        // Enforce upper bounds to prevent stats exceeding maximum values
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
        
        // ??? Consider adding lower bounds enforcement:
        // if (Health < 0) Health = 0;
        // if (Happiness < 0) Happiness = 0;
        // if (Hunger < 0) Hunger = 0;
        // Money might legitimately go negative (debt) so may not need bounds
    }
    
    #endregion

    #region String Representation
    
    /// <summary>
    /// Generates a contextual string representation of the pet for AI interactions
    /// 
    /// Creates a natural language description of the pet's current state that
    /// can be appended to AI prompts to provide context about the pet's condition.
    /// This helps the AI generate appropriate responses that reflect the pet's
    /// current physical and emotional state.
    /// 
    /// The generated text focuses on the current status descriptors rather than
    /// raw numeric values, making it more suitable for AI language processing.
    /// 
    /// ??? Commented out sections suggest there may have been more detailed
    /// information included previously (name, species, age, numeric values).
    /// Current implementation may be simplified for token efficiency in AI processing.
    /// </summary>
    /// <returns>
    /// Human-readable string describing the pet's current state using status text
    /// (e.g., "You are healthy and happy and full.")
    /// </returns>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        
        // Original comprehensive format (commented out):
        // sb.Append($"You are a {Species} who is named {Name} aged {Age}.");
        // sb.Append($"Your health is at {Health}, Your happiness is at {Happiness}, and Your hunger is at {Hunger}. You are {HealthText} and {HappinessText} and {HungerText}.");
        
        // Current simplified format for AI efficiency:
        sb.Append($"You are {HealthText} and {HappinessText} and {HungerText}.");
        
        return sb.ToString();
    }
    
    #endregion
}