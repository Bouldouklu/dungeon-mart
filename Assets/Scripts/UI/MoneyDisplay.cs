using UnityEngine;
using TMPro;

public class MoneyDisplay : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI moneyText;

    private void Start() {
        UpdateDisplay();
    }

    private void Update() {
        UpdateDisplay();
    }

    private void UpdateDisplay() {
        if (GameManager.Instance != null && moneyText != null) {
            moneyText.text = "$" + GameManager.Instance.CurrentMoney;
        }
    }
}