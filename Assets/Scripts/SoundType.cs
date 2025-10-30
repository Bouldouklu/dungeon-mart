/// <summary>
/// Defines all available sound effect types in the game.
/// Used by AudioManager to categorize and play sounds.
/// </summary>
public enum SoundType {
    // Gameplay Sounds
    CashRegister,
    DoorBell,
    BoxOpen,
    ShelfRestock,
    ShelfEmpty, // Phase 1: Alert sound when shelf becomes empty

    // UI Sounds
    UIClick,
    UIConfirm,
    UICancel,
    UIError,
    Purchase
}
