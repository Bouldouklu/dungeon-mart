using UnityEngine;

/// <summary>
/// Delivery box that contains items for the player's inventory.
/// Implements IInteractable to allow mouse-click based opening.
/// </summary>
public class DeliveryBox : MonoBehaviour, IInteractable {
    [Header("Box Contents")]
    private ItemDataSO itemData;

    private int quantity;

    [Header("Interaction Visual Feedback")]
    [SerializeField] private OutlineEffect outlineEffect;

    public void Initialize(ItemDataSO data, int qty) {
        itemData = data;
        quantity = qty;
        Debug.Log($"Delivery box initialized: {quantity}x {itemData.itemName}");
    }

    private void OpenBox() {
        if (itemData == null) {
            Debug.LogWarning("Cannot open box - no item data!");
            return;
        }

        // Play box open sound
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySound(SoundType.BoxOpen);
        }

        // Add items to inventory
        InventoryManager.Instance.AddToInventory(itemData, quantity);
        Debug.Log($"Opened delivery box: {quantity}x {itemData.itemName} added to inventory");

        // Notify supply chain manager
        if (SupplyChainManager.Instance != null) {
            SupplyChainManager.Instance.OnBoxOpened(this);
        }

        // Destroy box
        Destroy(gameObject);
    }

    #region IInteractable Implementation

    /// <summary>
    /// Called when mouse cursor hovers over the delivery box.
    /// Shows visual feedback via outline effect.
    /// </summary>
    public void OnHoverEnter()
    {
        if (outlineEffect != null)
        {
            outlineEffect.ShowOutline();
        }
    }

    /// <summary>
    /// Called when mouse cursor leaves the delivery box.
    /// Hides visual feedback.
    /// </summary>
    public void OnHoverExit()
    {
        if (outlineEffect != null)
        {
            outlineEffect.HideOutline();
        }
    }

    /// <summary>
    /// Called when the delivery box is clicked.
    /// Opens the box and adds items to inventory.
    /// </summary>
    public void OnClick()
    {
        OpenBox();
    }

    /// <summary>
    /// Returns the GameObject this interactable is attached to.
    /// Required by IInteractable interface.
    /// </summary>
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    #endregion
}