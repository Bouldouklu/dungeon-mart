using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a single upgrade card in the upgrade shop UI.
/// Displays upgrade info and handles visual states (locked, available, owned, maxed).
/// </summary>
public class UpgradeCard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Image backgroundImage;

    [Header("State Colors")]
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
    [SerializeField] private Color availableColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Green
    [SerializeField] private Color ownedColor = new Color(0.3f, 0.5f, 0.9f, 1f); // Blue
    [SerializeField] private Color maxedColor = new Color(1f, 0.84f, 0f, 1f); // Gold

    private UpgradeDataSO upgradeData;
    private System.Action<UpgradeDataSO> onCardClicked;

    /// <summary>
    /// Initializes the upgrade card with data and callback.
    /// </summary>
    public void Setup(UpgradeDataSO upgrade, System.Action<UpgradeDataSO> clickCallback)
    {
        Debug.Log($"🎴 UpgradeCard.Setup called for: {upgrade?.upgradeName ?? "null"}");

        upgradeData = upgrade;
        onCardClicked = clickCallback;

        // Setup button listener
        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(OnCardClicked);

            // Check if button has required components
            UnityEngine.UI.Image buttonImage = purchaseButton.GetComponent<UnityEngine.UI.Image>();
            bool hasImage = buttonImage != null;
            bool raycastTarget = hasImage && buttonImage.raycastTarget;

            Debug.Log($"✅ Button setup for: {upgrade.upgradeName}");
            Debug.Log($"   - Interactable: {purchaseButton.interactable}");
            Debug.Log($"   - Has Image: {hasImage}, Raycast Target: {raycastTarget}");
            Debug.Log($"   - RectTransform size: {((RectTransform)purchaseButton.transform).rect.size}");
        }
        else
        {
            Debug.LogError($"❌ purchaseButton is NULL for: {upgrade?.upgradeName}");
        }

        // Update visual state
        UpdateCardVisuals();
    }

    /// <summary>
    /// Updates all visual elements based on upgrade state.
    /// </summary>
    private void UpdateCardVisuals()
    {
        if (upgradeData == null) return;

        // Update icon
        if (iconImage != null && upgradeData.upgradeIcon != null)
        {
            iconImage.sprite = upgradeData.upgradeIcon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        // Update name
        if (nameText != null)
        {
            nameText.text = upgradeData.upgradeName;
        }

        // Update description
        if (descriptionText != null)
        {
            descriptionText.text = GetDescriptionWithEffect();
        }

        // Determine card state first (needed for cost color)
        UpgradeCardState state = GetCardState();

        // Update cost with color based on card state and affordability
        if (costText != null)
        {
            costText.text = $"${upgradeData.cost}";

            // Price color logic:
            // - Green: Available and can afford
            // - Red: Available but can't afford (insufficient funds only)
            // - Gray: Locked for other reasons (tier/prerequisites)
            if (state == UpgradeCardState.Available)
            {
                // Card is available - show green if affordable, red if not
                bool canAfford = GameManager.Instance != null && GameManager.Instance.CurrentMoney >= upgradeData.cost;
                costText.color = canAfford ? Color.green : Color.red;
            }
            else if (state == UpgradeCardState.Locked)
            {
                // Locked for tier/prereq reasons - show gray regardless of money
                costText.color = Color.gray;
            }
            else
            {
                // Owned/Maxed - show white/default
                costText.color = Color.white;
            }
        }

        // Apply state visuals
        ApplyStateVisuals(state);
    }

    /// <summary>
    /// Gets the description with effect information appended.
    /// </summary>
    private string GetDescriptionWithEffect()
    {
        string description = upgradeData.description;

        // Append effect info based on type
        switch (upgradeData.effectType)
        {
            case UpgradeEffectType.UnlockShopSegment:
                description += $"\n<i>Unlocks shop segment {upgradeData.targetSegmentIndex + 1}</i>";
                break;

            case UpgradeEffectType.IncreaseShelfCapacity:
                description += $"\n<i>+{upgradeData.effectValue} items per shelf slot</i>";
                break;

            case UpgradeEffectType.IncreaseCustomerCount:
                description += $"\n<i>+{upgradeData.effectValue} customers per day</i>";
                break;

            case UpgradeEffectType.DecreaseCheckoutTime:
                description += $"\n<i>{upgradeData.effectValue}% faster checkout</i>";
                break;

            case UpgradeEffectType.EnableBulkOrdering:
                description += $"\n<i>Order {upgradeData.effectValue}x items at once</i>";
                break;

            case UpgradeEffectType.EnableAutoRestock:
                description += $"\n<i>Automatic morning restocking</i>";
                break;
        }

        // Show purchase count for repeatable upgrades
        if (upgradeData.isRepeatable && UpgradeManager.Instance != null)
        {
            int purchaseCount = UpgradeManager.Instance.GetPurchaseCount(upgradeData.upgradeID);
            if (purchaseCount > 0)
            {
                description += $"\n<b>Owned: {purchaseCount}/{upgradeData.maxPurchases}</b>";
            }
        }

        return description;
    }

    /// <summary>
    /// Determines the current state of this upgrade card.
    /// </summary>
    private UpgradeCardState GetCardState()
    {
        if (UpgradeManager.Instance == null)
        {
            return UpgradeCardState.Locked;
        }

        // Check if locked by objective requirement
        if (upgradeData.requiredObjective != null)
        {
            if (ObjectiveManager.Instance == null || !ObjectiveManager.Instance.IsObjectiveCompleted(upgradeData.requiredObjective))
            {
                return UpgradeCardState.Locked;
            }
        }

        // Check if locked by prerequisites
        if (upgradeData.prerequisites != null && upgradeData.prerequisites.Length > 0)
        {
            foreach (UpgradeDataSO prereq in upgradeData.prerequisites)
            {
                if (prereq != null && !UpgradeManager.Instance.HasUpgrade(prereq.upgradeID))
                {
                    return UpgradeCardState.Locked;
                }
            }
        }

        // Check if maxed out (repeatable upgrades only)
        if (upgradeData.isRepeatable)
        {
            int purchaseCount = UpgradeManager.Instance.GetPurchaseCount(upgradeData.upgradeID);
            if (purchaseCount >= upgradeData.maxPurchases)
            {
                return UpgradeCardState.Maxed;
            }

            if (purchaseCount > 0)
            {
                return UpgradeCardState.Available; // Owned but can buy more
            }
        }
        else
        {
            // Non-repeatable upgrade - check if owned
            if (UpgradeManager.Instance.HasUpgrade(upgradeData.upgradeID))
            {
                return UpgradeCardState.Owned;
            }
        }

        // Check if player has enough money
        if (GameManager.Instance.CurrentMoney < upgradeData.cost)
        {
            return UpgradeCardState.Locked; // Can't afford = locked state
        }

        return UpgradeCardState.Available;
    }

    /// <summary>
    /// Applies visual styling based on card state.
    /// </summary>
    private void ApplyStateVisuals(UpgradeCardState state)
    {
        Color stateColor = lockedColor;
        string statusLabel = "LOCKED";

        switch (state)
        {
            case UpgradeCardState.Locked:
                stateColor = lockedColor;
                statusLabel = GetLockReason();
                break;

            case UpgradeCardState.Available:
                stateColor = availableColor;
                statusLabel = "AVAILABLE";
                break;

            case UpgradeCardState.Owned:
                stateColor = ownedColor;
                statusLabel = "OWNED";
                break;

            case UpgradeCardState.Maxed:
                stateColor = maxedColor;
                statusLabel = "MAXED OUT";
                break;
        }

        // Apply color to background
        if (backgroundImage != null)
        {
            backgroundImage.color = stateColor;
        }

        // Update status text
        if (statusText != null)
        {
            statusText.text = statusLabel;
        }

        // ALWAYS make button interactable - we handle requirements in click handler
        // This allows players to click and get feedback on why they can't purchase
        if (purchaseButton != null)
        {
            // Only disable for already-owned or maxed-out upgrades
            bool shouldBeClickable = (state != UpgradeCardState.Owned && state != UpgradeCardState.Maxed);
            purchaseButton.interactable = shouldBeClickable;
            Debug.Log($"🎴 Setting {upgradeData?.upgradeName} interactable to: {shouldBeClickable} (state: {state})");
        }
    }

    /// <summary>
    /// Gets a human-readable reason why this upgrade is locked.
    /// </summary>
    private string GetLockReason()
    {
        if (UpgradeManager.Instance == null)
        {
            return "LOCKED";
        }

        // Check objective requirement
        if (upgradeData.requiredObjective != null)
        {
            if (ObjectiveManager.Instance == null || !ObjectiveManager.Instance.IsObjectiveCompleted(upgradeData.requiredObjective))
            {
                return $"Requires: {upgradeData.requiredObjective.objectiveName}";
            }
        }

        // Check prerequisites
        if (upgradeData.prerequisites != null && upgradeData.prerequisites.Length > 0)
        {
            foreach (UpgradeDataSO prereq in upgradeData.prerequisites)
            {
                if (prereq != null && !UpgradeManager.Instance.HasUpgrade(prereq.upgradeID))
                {
                    return $"Requires: {prereq.upgradeName}";
                }
            }
        }

        // Check money
        if (GameManager.Instance.CurrentMoney < upgradeData.cost)
        {
            return "INSUFFICIENT FUNDS";
        }

        return "LOCKED";
    }

    /// <summary>
    /// Called when the card is clicked.
    /// </summary>
    private void OnCardClicked()
    {
        Debug.Log($"🎴 UpgradeCard.OnCardClicked called for: {upgradeData?.upgradeName ?? "null"}");
        Debug.Log($"🎴 Button info: interactable={purchaseButton?.interactable}, enabled={purchaseButton?.enabled}, gameObject.activeInHierarchy={purchaseButton?.gameObject.activeInHierarchy}");
        onCardClicked?.Invoke(upgradeData);
    }

    private void OnDestroy()
    {
        // Cleanup button listener
        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveAllListeners();
        }
    }
}

/// <summary>
/// Enum representing the visual state of an upgrade card.
/// </summary>
public enum UpgradeCardState
{
    Locked,      // Gray - Cannot purchase (tier/prereq/money requirement not met)
    Available,   // Green - Can purchase now
    Owned,       // Blue - Already purchased (non-repeatable)
    Maxed        // Gold - Maximum purchases reached (repeatable)
}
