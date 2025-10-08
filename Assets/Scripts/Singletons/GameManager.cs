using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    [SerializeField] private int currentMoney = 500;

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
        Debug.Log($"Money: ${currentMoney}");
    }

    public bool SpendMoney(int amount) {
        if (currentMoney < amount) {
            Debug.Log("Not enough money!");
            return false;
        }

        currentMoney -= amount;
        Debug.Log($"Money: ${currentMoney}");
        return true;
    }
}