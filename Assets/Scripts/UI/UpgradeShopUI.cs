using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the upgrade shop UI where players can purchase permanent upgrades.
/// Displays upgrade cards with filtering by category and tier requirements.
/// </summary>
public class UpgradeShopUI : MonoBehaviour
{
    public static UpgradeShopUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject upgradeShopPanel;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject upgradeCardPrefab;
    [SerializeField] private Button closeButton;

    [Header("Filter UI")]
    [SerializeField] private Button filterShopExpansionButton;
    [SerializeField] private Button filterShelvesButton;
    [SerializeField] private Button filterOperationsButton;
    [SerializeField] private Button filterCustomerFlowButton;
    [SerializeField] private Button filterLicensesButton;

    [Header("Confirmation Dialog")]
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("Settings")]
    [SerializeField] private List<UpgradeDataSO> availableUpgrades = new List<UpgradeDataSO>();

    private List<GameObject> spawnedCards = new List<GameObject>();
    private UpgradeCategory currentFilter = UpgradeCategory.ShopExpansion;
    private UpgradeDataSO pendingPurchase = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Ensure UI is hidden on start
        if (upgradeShopPanel != null)
        {
            upgradeShopPanel.SetActive(false);
        }

        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }

        // Setup button listeners
        SetupButtonListeners();
    }

    private void Start()
    {
        // Subscribe to objective events to refresh available upgrades
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveCompleted += OnObjectiveCompleted;
        }

        // Subscribe to upgrade events to refresh UI
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradePurchased += OnUpgradePurchased;
        }
    }

    private void OnDestroy()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveCompleted -= OnObjectiveCompleted;
        }

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradePurchased -= OnUpgradePurchased;
        }

        // Cleanup button listeners
        CleanupButtonListeners();

        // Ensure player movement is re-enabled if UI is destroyed
        EnablePlayerMovement();
    }

    private void SetupButtonListeners()
    {
        Debug.Log("🔧 SetupButtonListeners called");

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideUpgradeShop);
            Debug.Log("✅ Close button listener added");
        }

        if (filterShopExpansionButton != null)
        {
            filterShopExpansionButton.onClick.AddListener(() => SetFilter(UpgradeCategory.ShopExpansion));
            Debug.Log("✅ FilterShopExpansion button listener added");
        }

        if (filterShelvesButton != null)
        {
            filterShelvesButton.onClick.AddListener(() => SetFilter(UpgradeCategory.ShelfCapacity));
            Debug.Log("✅ FilterShelves button listener added");
        }

        if (filterOperationsButton != null)
        {
            filterOperationsButton.onClick.AddListener(() => SetFilter(UpgradeCategory.Operations));
            Debug.Log("✅ FilterOperations button listener added");
        }

        if (filterCustomerFlowButton != null)
        {
            filterCustomerFlowButton.onClick.AddListener(() => SetFilter(UpgradeCategory.CustomerFlow));
            Debug.Log("✅ FilterCustomerFlow button listener added");
        }

        if (filterLicensesButton != null)
        {
            filterLicensesButton.onClick.AddListener(() => SetFilter(UpgradeCategory.Licenses));
            Debug.Log("✅ FilterLicenses button listener added");
        }

        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.AddListener(ConfirmPurchase);
            Debug.Log("✅ ConfirmYes button listener added");
        }

        if (confirmNoButton != null)
        {
            confirmNoButton.onClick.AddListener(CancelPurchase);
            Debug.Log("✅ ConfirmNo button listener added");
        }
    }

    private void CleanupButtonListeners()
    {
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();
        if (filterShopExpansionButton != null) filterShopExpansionButton.onClick.RemoveAllListeners();
        if (filterShelvesButton != null) filterShelvesButton.onClick.RemoveAllListeners();
        if (filterOperationsButton != null) filterOperationsButton.onClick.RemoveAllListeners();
        if (filterCustomerFlowButton != null) filterCustomerFlowButton.onClick.RemoveAllListeners();
        if (filterLicensesButton != null) filterLicensesButton.onClick.RemoveAllListeners();
        if (confirmYesButton != null) confirmYesButton.onClick.RemoveAllListeners();
        if (confirmNoButton != null) confirmNoButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Shows the upgrade shop UI.
    /// </summary>
    public void ShowUpgradeShop()
    {
        if (upgradeShopPanel == null)
        {
            Debug.LogWarning("UpgradeShopUI: upgradeShopPanel is not assigned!");
            return;
        }

        // Refresh the upgrade list
        RefreshUpgradeCards();

        // Show the panel
        upgradeShopPanel.SetActive(true);

        // Force UpgradeShopPanel to render on top of PauseMenuPanel
        upgradeShopPanel.transform.SetAsLastSibling();
        Debug.Log("✅ UpgradeShopPanel set as last sibling (rendering on top)");

        // Disable player movement while UI is open
        DisablePlayerMovement();

        // Play UI open sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(SoundType.UIClick);
        }
    }

    /// <summary>
    /// Hides the upgrade shop UI.
    /// </summary>
    public void HideUpgradeShop()
    {
        if (upgradeShopPanel != null)
        {
            upgradeShopPanel.SetActive(false);
        }

        ClearUpgradeCards();

        // Re-enable player movement when UI closes
        EnablePlayerMovement();

        // Play UI close sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(SoundType.UICancel);
        }
    }

    /// <summary>
    /// Checks if the upgrade shop UI is currently open.
    /// </summary>
    public bool IsShopOpen()
    {
        return upgradeShopPanel != null && upgradeShopPanel.activeSelf;
    }

    /// <summary>
    /// Sets the category filter and refreshes the UI.
    /// </summary>
    private void SetFilter(UpgradeCategory category)
    {
        Debug.Log($"🔘 SetFilter called: category={category}");

        currentFilter = category;
        RefreshUpgradeCards();
    }

    /// <summary>
    /// Refreshes all upgrade cards based on current filter.
    /// </summary>
    private void RefreshUpgradeCards()
    {
        Debug.Log("📋 RefreshUpgradeCards called");
        ClearUpgradeCards();

        if (UpgradeManager.Instance == null)
        {
            Debug.LogWarning("UpgradeShopUI: UpgradeManager not found!");
            return;
        }

        // Get filtered upgrades
        List<UpgradeDataSO> filteredUpgrades = GetFilteredUpgrades();
        Debug.Log($"📋 Found {filteredUpgrades.Count} filtered upgrades");

        // Sort upgrades by category, then by cost
        filteredUpgrades = filteredUpgrades
            .OrderBy(u => u.category)
            .ThenBy(u => u.cost)
            .ToList();

        // Create cards for each upgrade
        foreach (UpgradeDataSO upgrade in filteredUpgrades)
        {
            Debug.Log($"📋 Creating card for: {upgrade.upgradeName}");
            CreateUpgradeCard(upgrade);
        }
    }

    /// <summary>
    /// Gets upgrades filtered by current category filter.
    /// </summary>
    private List<UpgradeDataSO> GetFilteredUpgrades()
    {
        List<UpgradeDataSO> filtered = availableUpgrades
            .Where(u => u.category == currentFilter)
            .ToList();

        Debug.Log($"📋 Filtering by category: {currentFilter}, found {filtered.Count} upgrades");

        return filtered;
    }

    /// <summary>
    /// Creates an upgrade card UI element.
    /// </summary>
    private void CreateUpgradeCard(UpgradeDataSO upgrade)
    {
        if (upgradeCardPrefab == null || cardContainer == null) return;

        GameObject cardObj = Instantiate(upgradeCardPrefab, cardContainer);
        spawnedCards.Add(cardObj);

        // Setup the card component
        UpgradeCard card = cardObj.GetComponent<UpgradeCard>();
        if (card != null)
        {
            card.Setup(upgrade, OnUpgradeCardClicked);
        }
        else
        {
            Debug.LogWarning("UpgradeCard component not found on prefab!");
        }
    }

    /// <summary>
    /// Clears all spawned upgrade cards.
    /// </summary>
    private void ClearUpgradeCards()
    {
        foreach (GameObject card in spawnedCards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }
        spawnedCards.Clear();
    }

    /// <summary>
    /// Called when player clicks an upgrade card.
    /// </summary>
    private void OnUpgradeCardClicked(UpgradeDataSO upgrade)
    {
        Debug.Log($"🔘 OnUpgradeCardClicked called: {upgrade?.upgradeName ?? "null"}");

        if (upgrade == null || UpgradeManager.Instance == null) return;

        // Check if upgrade can be purchased
        if (!UpgradeManager.Instance.CanPurchaseUpgrade(upgrade, out string reason))
        {
            // Show feedback why purchase failed
            Debug.Log($"Cannot purchase {upgrade.upgradeName}: {reason}");

            // Play error sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.UIError);
            }

            return;
        }

        // Show confirmation dialog
        ShowConfirmationDialog(upgrade);
    }

    /// <summary>
    /// Shows the purchase confirmation dialog.
    /// </summary>
    private void ShowConfirmationDialog(UpgradeDataSO upgrade)
    {
        if (confirmationDialog == null || confirmationText == null) return;

        pendingPurchase = upgrade;

        int remainingMoney = GameManager.Instance.CurrentMoney - upgrade.cost;
        string message = $"Purchase <b>{upgrade.upgradeName}</b> for <b>${upgrade.cost}</b>?\n\n";
        message += $"Remaining money: <b>${remainingMoney}</b>";

        confirmationText.text = message;
        confirmationDialog.SetActive(true);
    }

    /// <summary>
    /// Confirms the upgrade purchase.
    /// </summary>
    private void ConfirmPurchase()
    {
        if (pendingPurchase == null || UpgradeManager.Instance == null) return;

        bool success = UpgradeManager.Instance.PurchaseUpgrade(pendingPurchase);

        if (success)
        {
            Debug.Log($"✅ Successfully purchased: {pendingPurchase.upgradeName}");

            // Play purchase sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.Purchase);
            }

            // Refresh UI to show updated state
            RefreshUpgradeCards();
        }
        else
        {
            Debug.LogWarning($"Failed to purchase: {pendingPurchase.upgradeName}");

            // Play error sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.UIError);
            }
        }

        // Close confirmation dialog
        CancelPurchase();
    }

    /// <summary>
    /// Cancels the upgrade purchase.
    /// </summary>
    private void CancelPurchase()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }

        pendingPurchase = null;
    }

    /// <summary>
    /// Gets a human-readable reason why an upgrade cannot be purchased.
    /// </summary>
    private string GetPurchaseFailureReason(UpgradeDataSO upgrade)
    {
        if (UpgradeManager.Instance == null)
        {
            return "System error";
        }

        // Check objective requirement
        if (upgrade.requiredObjective != null)
        {
            if (ObjectiveManager.Instance == null)
            {
                return "System error";
            }

            if (!ObjectiveManager.Instance.IsObjectiveCompleted(upgrade.requiredObjective))
            {
                return $"Requires: {upgrade.requiredObjective.objectiveName}";
            }
        }

        // Check prerequisites
        if (upgrade.prerequisites != null && upgrade.prerequisites.Length > 0)
        {
            foreach (UpgradeDataSO prereq in upgrade.prerequisites)
            {
                if (prereq != null && !UpgradeManager.Instance.HasUpgrade(prereq.upgradeID))
                {
                    return $"Requires: {prereq.upgradeName}";
                }
            }
        }

        // Check money
        if (GameManager.Instance.CurrentMoney < upgrade.cost)
        {
            return $"Insufficient funds (need ${upgrade.cost})";
        }

        // Check if already owned (non-repeatable)
        if (!upgrade.isRepeatable && UpgradeManager.Instance.HasUpgrade(upgrade.upgradeID))
        {
            return "Already owned";
        }

        // Check purchase limit (repeatable)
        if (upgrade.isRepeatable && UpgradeManager.Instance.GetPurchaseCount(upgrade.upgradeID) >= upgrade.maxPurchases)
        {
            return "Maximum purchases reached";
        }

        return "Unknown reason";
    }

    /// <summary>
    /// Called when player completes an objective.
    /// </summary>
    private void OnObjectiveCompleted(ObjectiveDataSO completedObjective)
    {
        // Refresh UI to show newly unlocked upgrades
        if (IsShopOpen())
        {
            RefreshUpgradeCards();
        }
    }

    /// <summary>
    /// Called when an upgrade is purchased.
    /// </summary>
    private void OnUpgradePurchased(UpgradeDataSO upgrade, int purchaseCount)
    {
        // Refresh UI to show updated purchase state
        if (IsShopOpen())
        {
            RefreshUpgradeCards();
        }
    }

    /// <summary>
    /// Disables player movement (called when UI opens).
    /// </summary>
    private void DisablePlayerMovement()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetCanMove(false);
        }
    }

    /// <summary>
    /// Enables player movement (called when UI closes).
    /// </summary>
    private void EnablePlayerMovement()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetCanMove(true);
        }
    }
}