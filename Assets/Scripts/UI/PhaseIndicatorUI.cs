using UnityEngine;
using TMPro;

public class PhaseIndicatorUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TextMeshProUGUI dayText;

    private void Start() {
        if (DayManager.Instance != null) {
            DayManager.Instance.OnPhaseChanged += UpdatePhaseDisplay;
            DayManager.Instance.OnDayStarted += UpdateDayDisplay;
        }

        // Initial display
        UpdateDisplay();
    }

    private void OnDestroy() {
        if (DayManager.Instance != null) {
            DayManager.Instance.OnPhaseChanged -= UpdatePhaseDisplay;
            DayManager.Instance.OnDayStarted -= UpdateDayDisplay;
        }
    }

    private void UpdatePhaseDisplay(GamePhase phase) {
        UpdateDisplay();
    }

    private void UpdateDayDisplay(int day) {
        UpdateDisplay();
    }

    private void UpdateDisplay() {
        if (DayManager.Instance == null) return;

        GamePhase phase = DayManager.Instance.CurrentPhase;
        int day = DayManager.Instance.CurrentDay;

        // Update day text
        if (dayText != null) {
            dayText.text = $"Day {day}";
        }

        // Update phase text
        if (phaseText != null) {
            switch (phase) {
                case GamePhase.Morning:
                    phaseText.text = "MORNING";
                    break;

                case GamePhase.OpenForBusiness:
                    phaseText.text = "OPEN FOR BUSINESS";
                    break;

                case GamePhase.EndOfDay:
                    phaseText.text = "DAY COMPLETE";
                    break;
            }
        }
    }
}