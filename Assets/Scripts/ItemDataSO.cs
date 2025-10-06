using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "DungeonMart/Item Data")]
public class ItemDataSO : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;
    public int sellPrice = 10;
    public int restockCost = 5;
}
