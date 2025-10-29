using UnityEngine;
using UnityEngine.UI;

namespace DungeonMart3D
{
    /// <summary>
    /// Manages HUD buttons for Order and Upgrades panels.
    /// Enables/disables buttons based on the current game phase.
    /// </summary>
    public class HUDButtonManager : MonoBehaviour
    {
        [Header("Button References")]
        [SerializeField] private Button orderButton;
        [SerializeField] private Button upgradesButton;
        [SerializeField] private Button objectivesButton;

        [Header("Visual Feedback Settings")]
        [SerializeField] private float disabledAlpha = 0.5f;

        private CanvasGroup orderButtonCanvasGroup;
        private CanvasGroup upgradesButtonCanvasGroup;
        private CanvasGroup objectivesButtonCanvasGroup;

        private void Awake()
        {
            // Ensure buttons have CanvasGroup components for visual feedback
            SetupCanvasGroups();

            // Wire up button onClick events
            if (orderButton != null)
            {
                orderButton.onClick.AddListener(OnOrderButtonClicked);
            }
            else
            {
                Debug.LogError("HUDButtonManager: Order button reference is missing!");
            }

            if (upgradesButton != null)
            {
                upgradesButton.onClick.AddListener(OnUpgradesButtonClicked);
            }
            else
            {
                Debug.LogError("HUDButtonManager: Upgrades button reference is missing!");
            }

            if (objectivesButton != null)
            {
                objectivesButton.onClick.AddListener(OnObjectivesButtonClicked);
            }
            else
            {
                Debug.LogError("HUDButtonManager: Objectives button reference is missing!");
            }

            // Set initial state to disabled
            UpdateButtonStates(GamePhase.Morning);
        }

        private void Start()
        {
            // Subscribe to phase change events
            if (DayManager.Instance != null)
            {
                DayManager.Instance.OnPhaseChanged += OnPhaseChanged;

                // Update to current phase
                UpdateButtonStates(DayManager.Instance.CurrentPhase);
            }
            else
            {
                Debug.LogError("HUDButtonManager: DayManager instance not found!");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (DayManager.Instance != null)
            {
                DayManager.Instance.OnPhaseChanged -= OnPhaseChanged;
            }

            // Remove button listeners
            if (orderButton != null)
            {
                orderButton.onClick.RemoveListener(OnOrderButtonClicked);
            }

            if (upgradesButton != null)
            {
                upgradesButton.onClick.RemoveListener(OnUpgradesButtonClicked);
            }

            if (objectivesButton != null)
            {
                objectivesButton.onClick.RemoveListener(OnObjectivesButtonClicked);
            }
        }

        private void SetupCanvasGroups()
        {
            // Add or get CanvasGroup for order button
            if (orderButton != null)
            {
                orderButtonCanvasGroup = orderButton.GetComponent<CanvasGroup>();
                if (orderButtonCanvasGroup == null)
                {
                    orderButtonCanvasGroup = orderButton.gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Add or get CanvasGroup for upgrades button
            if (upgradesButton != null)
            {
                upgradesButtonCanvasGroup = upgradesButton.GetComponent<CanvasGroup>();
                if (upgradesButtonCanvasGroup == null)
                {
                    upgradesButtonCanvasGroup = upgradesButton.gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Add or get CanvasGroup for objectives button
            if (objectivesButton != null)
            {
                objectivesButtonCanvasGroup = objectivesButton.GetComponent<CanvasGroup>();
                if (objectivesButtonCanvasGroup == null)
                {
                    objectivesButtonCanvasGroup = objectivesButton.gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void OnPhaseChanged(GamePhase newPhase)
        {
            UpdateButtonStates(newPhase);
        }

        private void UpdateButtonStates(GamePhase currentPhase)
        {
            bool shouldEnableButtons = currentPhase == GamePhase.EndOfDay;

            SetButtonState(orderButton, orderButtonCanvasGroup, shouldEnableButtons);
            SetButtonState(upgradesButton, upgradesButtonCanvasGroup, shouldEnableButtons);

            // Objectives button is always enabled (not phase-restricted)
            SetButtonState(objectivesButton, objectivesButtonCanvasGroup, true);
        }

        private void SetButtonState(Button button, CanvasGroup canvasGroup, bool enabled)
        {
            if (button == null) return;

            button.interactable = enabled;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = enabled ? 1f : disabledAlpha;
            }
        }

        private void OnOrderButtonClicked()
        {
            if (OrderMenu.Instance != null)
            {
                OrderMenu.Instance.ToggleMenu();
            }
            else
            {
                Debug.LogError("HUDButtonManager: OrderMenu instance not found!");
            }
        }

        private void OnUpgradesButtonClicked()
        {
            if (UpgradeShopUI.Instance != null)
            {
                UpgradeShopUI.Instance.ShowUpgradeShop();
            }
            else
            {
                Debug.LogError("HUDButtonManager: UpgradeShopUI instance not found!");
            }
        }

        private void OnObjectivesButtonClicked()
        {
            // Find ObjectivesPanelUI in scene and toggle it
            ObjectivesPanelUI objectivesPanel = FindFirstObjectByType<ObjectivesPanelUI>();
            if (objectivesPanel != null)
            {
                objectivesPanel.TogglePanel();
            }
            else
            {
                Debug.LogError("HUDButtonManager: ObjectivesPanelUI not found in scene!");
            }
        }
    }
}
