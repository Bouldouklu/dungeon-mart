using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the loan selection UI modal.
/// Displays available loan options with interest calculations.
/// </summary>
public class LoanUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loanPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private TextMeshProUGUI interestRateText;
    [SerializeField] private Transform loanOptionsContainer;
    [SerializeField] private GameObject loanOptionButtonPrefab;
    [SerializeField] private Button closePanelButton;

    private int suggestedAmount = 0;

    private void Start()
    {
        // Hide panel initially
        if (loanPanel != null)
        {
            loanPanel.SetActive(false);
        }

        // Wire up close button
        if (closePanelButton != null)
        {
            closePanelButton.onClick.AddListener(CloseLoanPanel);
        }

        // Subscribe to loan taken event
        if (FinancialManager.Instance != null)
        {
            FinancialManager.Instance.OnLoanTaken += OnLoanTaken;
        }
    }

    private void OnDestroy()
    {
        if (FinancialManager.Instance != null)
        {
            FinancialManager.Instance.OnLoanTaken -= OnLoanTaken;
        }
    }

    /// <summary>
    /// Shows the loan panel with available loan options.
    /// </summary>
    /// <param name="suggestedMinimum">Minimum amount suggested (e.g., to pay rent)</param>
    public void ShowLoanPanel(int suggestedMinimum = 0)
    {
        if (loanPanel == null)
        {
            Debug.LogError("Loan panel is not assigned!");
            return;
        }

        // Check if player can take a loan
        if (FinancialManager.Instance == null || !FinancialManager.Instance.CanTakeLoan())
        {
            Debug.LogWarning("Cannot take loan - already have active loan!");
            return;
        }

        suggestedAmount = suggestedMinimum;
        loanPanel.SetActive(true);

        // Pause player movement
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetCanMove(false);
        }

        // Update title
        if (titleText != null)
        {
            titleText.text = "Emergency Loan Services";
        }

        // Update interest rate display
        if (interestRateText != null && FinancialManager.Instance != null)
        {
            float rate = FinancialManager.Instance.InterestRate * 100f;
            interestRateText.text = $"Interest Rate: {rate}% (We're definitely not loan sharks!)";
        }

        // Update warning text
        if (warningText != null)
        {
            if (suggestedAmount > 0)
            {
                warningText.text = $"You need at least ${suggestedAmount} to pay rent!";
                warningText.color = Color.yellow;
            }
            else
            {
                warningText.text = "Borrow wisely! Loans must be repaid within 7 days.";
                warningText.color = Color.white;
            }
        }

        // Populate loan options
        PopulateLoanOptions();
    }

    /// <summary>
    /// Populates the loan options based on available amounts.
    /// </summary>
    private void PopulateLoanOptions()
    {
        if (loanOptionsContainer == null || loanOptionButtonPrefab == null)
        {
            Debug.LogError("Loan options container or prefab not assigned!");
            return;
        }

        // Clear existing options
        foreach (Transform child in loanOptionsContainer)
        {
            Destroy(child.gameObject);
        }

        // Create loan option buttons
        if (FinancialManager.Instance != null)
        {
            int[] amounts = FinancialManager.Instance.AvailableLoanAmounts;
            foreach (int amount in amounts)
            {
                CreateLoanOptionButton(amount);
            }
        }
    }

    /// <summary>
    /// Creates a loan option button for the specified amount.
    /// </summary>
    private void CreateLoanOptionButton(int loanAmount)
    {
        GameObject buttonObj = Instantiate(loanOptionButtonPrefab, loanOptionsContainer);
        Button button = buttonObj.GetComponent<Button>();

        // Calculate total with interest
        int totalOwed = FinancialManager.Instance.CalculateLoanWithInterest(loanAmount);
        int interestAmount = totalOwed - loanAmount;

        // Find text components (assuming button has TextMeshProUGUI children)
        TextMeshProUGUI[] texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 1)
        {
            // Main text: loan amount
            texts[0].text = $"Borrow ${loanAmount}";
        }
        if (texts.Length >= 2)
        {
            // Detail text: total repayment
            texts[1].text = $"Repay ${totalOwed} (${interestAmount} interest)";
        }

        // Highlight suggested amount
        bool isSuggested = loanAmount >= suggestedAmount && suggestedAmount > 0;
        if (isSuggested)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.3f, 0.8f, 0.3f); // Green tint
            button.colors = colors;
        }

        // Wire up click event
        button.onClick.AddListener(() => OnLoanAmountSelected(loanAmount));
    }

    /// <summary>
    /// Called when a loan amount is selected.
    /// </summary>
    private void OnLoanAmountSelected(int amount)
    {
        if (FinancialManager.Instance != null)
        {
            bool success = FinancialManager.Instance.TakeLoan(amount);
            if (success)
            {
                Debug.Log($"Loan taken: ${amount}");

                // Play UI confirm sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.UIConfirm);
                }

                // Panel will close via OnLoanTaken event
            }
            else
            {
                Debug.LogError($"Failed to take loan of ${amount}");

                // Play error sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.UIError);
                }
            }
        }
    }

    /// <summary>
    /// Called when a loan is successfully taken.
    /// </summary>
    private void OnLoanTaken(int amount, int totalOwed, int daysUntilDue)
    {
        CloseLoanPanel();

        // Try to pay rent automatically if that's why we took the loan
        if (FinancialManager.Instance != null && FinancialManager.Instance.RentIsDueNow)
        {
            bool rentPaid = FinancialManager.Instance.PayRent();
            if (rentPaid)
            {
                Debug.Log("Rent automatically paid with loan funds!");
            }
        }
    }

    /// <summary>
    /// Closes the loan panel.
    /// </summary>
    private void CloseLoanPanel()
    {
        // Play UI close sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(SoundType.UICancel);
        }

        if (loanPanel != null)
        {
            loanPanel.SetActive(false);
        }

        // Resume player movement
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetCanMove(true);
        }
    }
}
