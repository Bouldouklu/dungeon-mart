using UnityEngine;
using TMPro;

public class PhaseIndicatorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI instructionText;

    private void Start()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnPhaseChanged += UpdatePhaseDisplay;
            DayManager.Instance.OnDayStarted += UpdateDayDisplay;
        }

        // Initial display
        UpdateDisplay();
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnPhaseChanged -= UpdatePhaseDisplay;
            DayManager.Instance.OnDayStarted -= UpdateDayDisplay;
        }
    }

    private void UpdatePhaseDisplay(GamePhase phase)
    {
        UpdateDisplay();
    }

    private void UpdateDayDisplay(int day)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (DayManager.Instance == null) return;

        GamePhase phase = DayManager.Instance.CurrentPhase;
        int day = DayManager.Instance.CurrentDay;

        // Update day text
        if (dayText != null)
        {
            dayText.text = $"Day {day}";
        }

        // Update phase text and instructions
        if (phaseText != null && instructionText != null)
        {
            switch (phase)
            {
                case GamePhase.Morning:
                    phaseText.text = "MORNING";
                    instructionText.text = "Open delivery boxes (E)\nRestock shelves (E)\nPress O to open shop";
                    break;

                case GamePhase.OpenForBusiness:
                    phaseText.text = "OPEN FOR BUSINESS";
                    instructionText.text = "Serve customers and keep shelves stocked!\nDebug: Press K to end day";
                    break;

                case GamePhase.EndOfDay:
                    phaseText.text = "DAY COMPLETE";
                    instructionText.text = "Press Tab to place orders\nDebug: Press M for next morning";
                    break;
            }
        }
    }
}
