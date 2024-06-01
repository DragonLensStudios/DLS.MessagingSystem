namespace DLS.MessageSystem.Messaging
{
public enum MessageChannels
{
    // System related messages
    System,
    Logging,
    Debug,
    Error,
    Warning,
    Info,

    // Gameplay related messages
    Gameplay,
    Level,
    Quest,
    Achievement,
    Combat,
    AI,
    Physics,
    Animation,

    // Player related messages
    Player,
    PlayerStats,
    PlayerInventory,
    PlayerSkills,
    PlayerActions,
    PlayerHealth,
    PlayerMovement,

    // Enemy related messages
    Enemy,
    EnemyStats,
    EnemyAI,
    EnemyHealth,
    EnemyActions,

    // Items and Inventory related messages
    Items,
    ItemPickup,
    ItemDrop,
    ItemUse,
    ItemCrafting,
    InventoryManagement,

    // UI related messages
    UI,
    UINotifications,
    UIMenus,
    UIButtons,
    UIDialogs,
    UILoadingScreen,
    UIPopup,

    // Network related messages
    Network,
    NetworkConnect,
    NetworkDisconnect,
    NetworkError,
    NetworkData,

    // Audio related messages
    Audio,
    Music,
    SoundEffects,
    Voice,

    // Input related messages
    Input,
    KeyPress,
    MouseClick,
    Touch,

    // Time related messages
    Time,
    DayNightCycle,
    Timer,

    // Other miscellaneous categories
    Weather,
    Environment,
    NPC,
    Scripting,
    SaveLoad,
    Cutscene,
    Tutorial,
    Economy,
    Trade,
    Dialogue,
    Camera,
    UI_HUD,
    UI_Inventory,
    UI_QuestLog,
    UI_SkillTree,
    Social,
    Chat,
    Mail,
    FriendRequest,
    Clan,
    Group
}


}