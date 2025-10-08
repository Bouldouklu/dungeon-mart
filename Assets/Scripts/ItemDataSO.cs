using UnityEngine;

public enum ItemSize {
    Small,     // Potions, food - 1 slot
    Medium,     // Bags, swords, shields - 2 slots
    Big         // Traps, thrones, furniture - 3-4 slots
}

[CreateAssetMenu(fileName = "New Item", menuName = "DungeonMart/Item Data")]
public class ItemDataSO : ScriptableObject {
    public string itemName;
    public Sprite itemSprite;
    public int sellPrice = 10;
    public int restockCost = 5;

    [Header("Display Properties")]
    public ItemSize itemSize = ItemSize.Small;
    public int slotsRequired = 1;

    private void OnValidate() {
        // Auto-set slots based on size if not manually configured
        switch (itemSize) {
            case ItemSize.Small:
                if (slotsRequired == 1) slotsRequired = 1;
                break;
            case ItemSize.Medium:
                if (slotsRequired == 1) slotsRequired = 2;
                break;
            case ItemSize.Big:
                if (slotsRequired <= 2) slotsRequired = 3;
                break;
        }
    }
}