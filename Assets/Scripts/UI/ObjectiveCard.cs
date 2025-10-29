using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single objective card in the objectives panel.
/// Displays objective name, description, progress, and completion status.
/// </summary>
public class ObjectiveCard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI objectiveNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject completedBadge;
    [SerializeField] private TextMeshProUGUI unlocksText;

    [Header("State Colors")]
    [SerializeField] private Color inProgressColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color completedColor = new Color(0.1f, 0.3f, 0.1f, 1f);

    private ObjectiveDataSO objective;

    /// <summary>
    /// Sets up the card with objective data.
    /// </summary>
    public void SetupCard(ObjectiveDataSO objectiveData)
    {
        objective = objectiveData;

        if (objective == null)
        {
            Debug.LogError("[ObjectiveCard] Objective data is null!");
            return;
        }

        // Set objective name
        if (objectiveNameText != null)
        {
            objectiveNameText.text = objective.objectiveName;
        }

        // Set description
        if (descriptionText != null)
        {
            descriptionText.text = objective.description;
        }

        // Set icon
        if (iconImage != null && objective.objectiveIcon != null)
        {
            iconImage.sprite = objective.objectiveIcon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        // Set unlocks text
        if (unlocksText != null)
        {
            if (objective.unlocksUpgrade != null)
            {
                unlocksText.text = $"Unlocks: {objective.unlocksUpgrade.upgradeName}";
                unlocksText.gameObject.SetActive(true);
            }
            else
            {
                unlocksText.gameObject.SetActive(false);
            }
        }

        // Update progress and visual state
        RefreshCard();
    }

    /// <summary>
    /// Refreshes the card's progress and visual state.
    /// </summary>
    private void RefreshCard()
    {
        if (objective == null || ObjectiveManager.Instance == null)
            return;

        var (current, target, complete) = ObjectiveManager.Instance.GetObjectiveProgress(objective);

        UpdateProgress(current, target);
        UpdateVisualState(complete);
    }

    /// <summary>
    /// Updates the progress bar and text.
    /// </summary>
    public void UpdateProgress(int current, int target)
    {
        // Update progress bar
        if (progressBar != null)
        {
            float progress = target > 0 ? (float)current / target : 0f;
            progressBar.value = Mathf.Clamp01(progress);
        }

        // Update progress text
        if (progressText != null)
        {
            progressText.text = GetProgressText(current, target);
        }
    }

    /// <summary>
    /// Gets the formatted progress text based on objective type.
    /// </summary>
    private string GetProgressText(int current, int target)
    {
        if (objective == null)
            return "";

        bool isComplete = ObjectiveManager.Instance != null && ObjectiveManager.Instance.IsObjectiveCompleted(objective);

        if (isComplete)
        {
            return "COMPLETE!";
        }

        switch (objective.objectiveType)
        {
            case ObjectiveType.Revenue:
                return $"${current:N0} / ${target:N0}";

            case ObjectiveType.CustomersServed:
                return $"{current} / {target} Customers";

            case ObjectiveType.ItemsSold:
                string categoryName = objective.categoryFilter != ItemCategory.None
                    ? $" {objective.categoryFilter}"
                    : "";
                return $"{current} / {target}{categoryName} Sold";

            case ObjectiveType.DaysPlayed:
                return $"Day {current} / {target}";

            case ObjectiveType.Hybrid:
                return GetHybridProgressText(current, target);

            default:
                return $"{current} / {target}";
        }
    }

    /// <summary>
    /// Gets formatted progress text for hybrid objectives.
    /// </summary>
    private string GetHybridProgressText(int current, int target)
    {
        if (objective == null)
            return "";

        // Show the most relevant progress metric
        if (objective.requiredRevenue > 0)
        {
            return $"${current:N0} / ${target:N0}";
        }
        else if (objective.requiredCustomers > 0)
        {
            return $"{current} / {objective.requiredCustomers} Customers";
        }
        else if (objective.requiredItemsSold > 0)
        {
            return $"{current} / {objective.requiredItemsSold} Items";
        }
        else if (objective.requiredDays > 0)
        {
            return $"Day {current} / {objective.requiredDays}";
        }

        return $"{current} / {target}";
    }

    /// <summary>
    /// Updates the visual state based on completion status.
    /// </summary>
    private void UpdateVisualState(bool isComplete)
    {
        // Update background color
        if (backgroundImage != null)
        {
            backgroundImage.color = isComplete ? completedColor : inProgressColor;
        }

        // Show/hide completed badge
        if (completedBadge != null)
        {
            completedBadge.SetActive(isComplete);
        }

        // Update progress bar visibility
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(!isComplete);
        }
    }

    /// <summary>
    /// Gets the objective this card represents.
    /// </summary>
    public ObjectiveDataSO GetObjective()
    {
        return objective;
    }
}
