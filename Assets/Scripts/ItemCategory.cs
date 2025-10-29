using System;

/// <summary>
/// Defines the various categories of items available in DungeonMart.
/// Categories determine which shelves can hold items and unlock progression.
/// </summary>
[Serializable]
public enum ItemCategory
{
    Weapons = 0,
    Shields = 1,
    Potions = 2,
    ArmorApparel = 3,
    Traps = 4,
    MagicItems = 5
}
