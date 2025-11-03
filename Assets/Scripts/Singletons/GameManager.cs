using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    [SerializeField] private int currentMoney = 700;

    /// <summary>
    /// Event triggered when money is added to player's account.
    /// Parameters: amount added, new total
    /// </summary>
    public event System.Action<int, int> OnMoneyAdded;

    /// <summary>
    /// Event triggered when money is spent from player's account.
    /// Parameters: amount spent, new total
    /// </summary>
    public event System.Action<int, int> OnMoneySpent;

    /// <summary>
    /// Event triggered when player's money changes for any reason.
    /// Parameters: new total, change amount (positive or negative)
    /// </summary>
    public event System.Action<int, int> OnMoneyChanged;

    public int CurrentMoney => currentMoney;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddMoney(int amount) {
        currentMoney += amount;
        Debug.Log($"Money added: +${amount}. Total: ${currentMoney}");

        OnMoneyAdded?.Invoke(amount, currentMoney);
        OnMoneyChanged?.Invoke(currentMoney, amount);
    }

    public bool SpendMoney(int amount) {
        if (currentMoney < amount) {
            Debug.Log($"Not enough money! Need ${amount}, have ${currentMoney}");
            return false;
        }

        currentMoney -= amount;
        Debug.Log($"Money spent: -${amount}. Total: ${currentMoney}");

        OnMoneySpent?.Invoke(amount, currentMoney);
        OnMoneyChanged?.Invoke(currentMoney, -amount);
        return true;
    }

    /// <summary>
    /// Sets money to a specific amount (for debugging/testing).
    /// </summary>
    public void SetMoney(int amount) {
        int change = amount - currentMoney;
        currentMoney = amount;
        Debug.Log($"Money set to: ${currentMoney}");
        OnMoneyChanged?.Invoke(currentMoney, change);
    }
}