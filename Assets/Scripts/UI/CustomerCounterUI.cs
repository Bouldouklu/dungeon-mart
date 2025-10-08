using UnityEngine;
using TMPro;

public class CustomerCounterUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private GameObject counterPanel;

    private void Start() {
        if (DayManager.Instance != null) {
            DayManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }

        UpdateDisplay();
    }

    private void OnDestroy() {
        if (DayManager.Instance != null) {
            DayManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    private void OnPhaseChanged(GamePhase phase) {
        // Only show counter during business hours
        if (counterPanel != null) {
            counterPanel.SetActive(phase == GamePhase.OpenForBusiness);
        }

        if (phase == GamePhase.OpenForBusiness) {
            // Start updating counter
            InvokeRepeating(nameof(UpdateDisplay), 0f, 0.5f);
        }
        else {
            CancelInvoke(nameof(UpdateDisplay));
        }
    }

    private void UpdateDisplay() {
        if (CustomerSpawner.Instance == null || counterText == null) return;

        int left = CustomerSpawner.Instance.CustomersLeft;
        int total = CustomerSpawner.Instance.TotalCustomersForDay;

        if (total > 0) {
            counterText.text = $"Customers: {left}/{total}";
        }
        else {
            counterText.text = "Customers: 0/0";
        }
    }
}