using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the objectives panel UI displaying all objectives and their progress.
/// Shows objectives based on their reveal conditions and completion status.
/// </summary>
public class ObjectivesPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject objectiveCardPrefab;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton;

    [Header("Filter Buttons")]
    [SerializeField] private Button allButton;
    [SerializeField] private Button inProgressButton;
    [SerializeField] private Button completedButton;

    [Header("Settings")]
    [SerializeField] private bool showDebugLogs = false;

    private List<ObjectiveCard> objectiveCards = new List<ObjectiveCard>();
    private ObjectiveFilter currentFilter = ObjectiveFilter.All;

    private enum ObjectiveFilter
    {
        All,
        InProgress,
        Completed
    }

    private void Start()
    {
        // Setup button listeners
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        if (allButton != null)
        {
            allButton.onClick.AddListener(() => SetFilter(ObjectiveFilter.All));
        }

        if (inProgressButton != null)
        {
            inProgressButton.onClick.AddListener(() => SetFilter(ObjectiveFilter.InProgress));
        }

        if (completedButton != null)
        {
            completedButton.onClick.AddListener(() => SetFilter(ObjectiveFilter.Completed));
        }

        // Subscribe to objective events
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveCompleted += OnObjectiveCompleted;
            ObjectiveManager.Instance.OnObjectiveProgressChanged += OnObjectiveProgressChanged;
            ObjectiveManager.Instance.OnObjectiveRevealed += OnObjectiveRevealed;
        }

        // Start with panel closed
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Cleanup button listeners
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
        }

        if (allButton != null)
        {
            allButton.onClick.RemoveAllListeners();
        }

        if (inProgressButton != null)
        {
            inProgressButton.onClick.RemoveAllListeners();
        }

        if (completedButton != null)
        {
            completedButton.onClick.RemoveAllListeners();
        }

        // Unsubscribe from objective events
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveCompleted -= OnObjectiveCompleted;
            ObjectiveManager.Instance.OnObjectiveProgressChanged -= OnObjectiveProgressChanged;
            ObjectiveManager.Instance.OnObjectiveRevealed -= OnObjectiveRevealed;
        }
    }

    /// <summary>
    /// Opens the objectives panel and refreshes the display.
    /// </summary>
    public void OpenPanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            RefreshObjectiveCards();
            UpdateTitleText();

            if (showDebugLogs)
                Debug.Log("[ObjectivesPanelUI] Panel opened");
        }
    }

    /// <summary>
    /// Closes the objectives panel.
    /// </summary>
    public void ClosePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);

            if (showDebugLogs)
                Debug.Log("[ObjectivesPanelUI] Panel closed");
        }
    }

    /// <summary>
    /// Toggles the panel open/closed.
    /// </summary>
    public void TogglePanel()
    {
        if (panelRoot != null)
        {
            if (panelRoot.activeSelf)
                ClosePanel();
            else
                OpenPanel();
        }
    }

    /// <summary>
    /// Checks if the panel is currently open.
    /// </summary>
    public bool IsPanelOpen()
    {
        return panelRoot != null && panelRoot.activeSelf;
    }

    /// <summary>
    /// Sets the current filter and refreshes the display.
    /// </summary>
    private void SetFilter(ObjectiveFilter filter)
    {
        currentFilter = filter;
        RefreshObjectiveCards();
        UpdateFilterButtonStates();

        if (showDebugLogs)
            Debug.Log($"[ObjectivesPanelUI] Filter set to: {filter}");
    }

    /// <summary>
    /// Updates the visual state of filter buttons.
    /// </summary>
    private void UpdateFilterButtonStates()
    {
        // You can add visual feedback here (e.g., color changes)
        // For now, this is a placeholder for future button state updates
    }

    /// <summary>
    /// Refreshes all objective cards based on current filter.
    /// </summary>
    private void RefreshObjectiveCards()
    {
        if (ObjectiveManager.Instance == null)
        {
            Debug.LogWarning("[ObjectivesPanelUI] ObjectiveManager not found!");
            return;
        }

        // Clear existing cards
        ClearObjectiveCards();

        // Get visible objectives
        List<ObjectiveDataSO> visibleObjectives = ObjectiveManager.Instance.GetVisibleObjectives();

        // Apply filter
        visibleObjectives = ApplyFilter(visibleObjectives);

        // Sort objectives: incomplete first, then completed
        visibleObjectives = visibleObjectives
            .OrderBy(obj => ObjectiveManager.Instance.IsObjectiveCompleted(obj) ? 1 : 0)
            .ThenBy(obj => obj.objectiveName)
            .ToList();

        // Create cards
        foreach (ObjectiveDataSO objective in visibleObjectives)
        {
            CreateObjectiveCard(objective);
        }

        if (showDebugLogs)
            Debug.Log($"[ObjectivesPanelUI] Refreshed {objectiveCards.Count} objective cards");
    }

    /// <summary>
    /// Applies the current filter to the objectives list.
    /// </summary>
    private List<ObjectiveDataSO> ApplyFilter(List<ObjectiveDataSO> objectives)
    {
        switch (currentFilter)
        {
            case ObjectiveFilter.InProgress:
                return objectives.Where(obj => !ObjectiveManager.Instance.IsObjectiveCompleted(obj)).ToList();

            case ObjectiveFilter.Completed:
                return objectives.Where(obj => ObjectiveManager.Instance.IsObjectiveCompleted(obj)).ToList();

            case ObjectiveFilter.All:
            default:
                return objectives;
        }
    }

    /// <summary>
    /// Clears all objective cards from the container.
    /// </summary>
    private void ClearObjectiveCards()
    {
        foreach (ObjectiveCard card in objectiveCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        objectiveCards.Clear();
    }

    /// <summary>
    /// Creates a new objective card for the given objective.
    /// </summary>
    private void CreateObjectiveCard(ObjectiveDataSO objective)
    {
        if (objectiveCardPrefab == null || cardContainer == null)
        {
            Debug.LogWarning("[ObjectivesPanelUI] Card prefab or container not assigned!");
            return;
        }

        GameObject cardObject = Instantiate(objectiveCardPrefab, cardContainer);
        ObjectiveCard card = cardObject.GetComponent<ObjectiveCard>();

        if (card != null)
        {
            card.SetupCard(objective);
            objectiveCards.Add(card);
        }
        else
        {
            Debug.LogError("[ObjectivesPanelUI] ObjectiveCard component not found on prefab!");
            Destroy(cardObject);
        }
    }

    /// <summary>
    /// Updates the title text showing completion count.
    /// </summary>
    private void UpdateTitleText()
    {
        if (titleText == null || ObjectiveManager.Instance == null)
            return;

        int completedCount = ObjectiveManager.Instance.CompletedObjectiveCount;
        int visibleCount = ObjectiveManager.Instance.GetVisibleObjectives().Count;

        titleText.text = $"Objectives ({completedCount}/{visibleCount} Complete)";
    }

    #region Event Handlers

    /// <summary>
    /// Called when an objective is completed.
    /// </summary>
    private void OnObjectiveCompleted(ObjectiveDataSO objective)
    {
        if (IsPanelOpen())
        {
            RefreshObjectiveCards();
            UpdateTitleText();
        }

        if (showDebugLogs)
            Debug.Log($"[ObjectivesPanelUI] Objective completed: {objective.objectiveName}");
    }

    /// <summary>
    /// Called when objective progress changes.
    /// </summary>
    private void OnObjectiveProgressChanged(ObjectiveDataSO objective, int current, int target)
    {
        // Find and update the specific card
        ObjectiveCard card = objectiveCards.FirstOrDefault(c => c.GetObjective() == objective);
        if (card != null)
        {
            card.UpdateProgress(current, target);
        }
    }

    /// <summary>
    /// Called when a new objective is revealed.
    /// </summary>
    private void OnObjectiveRevealed(ObjectiveDataSO objective)
    {
        if (IsPanelOpen())
        {
            RefreshObjectiveCards();
            UpdateTitleText();
        }

        if (showDebugLogs)
            Debug.Log($"[ObjectivesPanelUI] Objective revealed: {objective.objectiveName}");
    }

    #endregion
}
