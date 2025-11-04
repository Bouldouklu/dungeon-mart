using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckoutCounter : MonoBehaviour {
    public static CheckoutCounter Instance { get; private set; }

    [SerializeField] private float transactionTime = 1f; // Phase 1: Reduced from 2s to compress business phase
    [SerializeField] private Transform checkoutPosition;

    private Queue<Customer> customerQueue = new Queue<Customer>();
    private Customer currentCustomer = null;
    private bool isProcessing = false;
    private float checkoutSpeedModifier = 1.0f; // 1.0 = normal speed, 0.75 = 25% faster

    // Event fired when items are sold (for objective tracking)
    public event Action<ItemDataSO, int> OnItemSold;

    private void Awake() {
        // Singleton pattern
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void JoinQueue(Customer customer) {
        customerQueue.Enqueue(customer);
        Debug.Log("Customer joined queue. Queue length: " + customerQueue.Count);

        // Start processing if not already doing so
        if (!isProcessing) {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue() {
        isProcessing = true;

        while (customerQueue.Count > 0) {
            currentCustomer = customerQueue.Dequeue();

            if (currentCustomer == null) {
                continue;
            }

            Debug.Log("Processing customer. Remaining in queue: " + customerQueue.Count);

            // Signal customer to approach counter
            Vector3 counterPosition = checkoutPosition != null ? checkoutPosition.position : transform.position;
            currentCustomer.ApproachCheckout(counterPosition);

            // Wait for customer to actually reach the counter
            while (currentCustomer != null && currentCustomer.IsMoving) {
                yield return null;
            }

            // Small delay after arriving
            yield return new WaitForSeconds(0.3f);

            // Process transaction (apply speed modifier from upgrades)
            yield return new WaitForSeconds(transactionTime * checkoutSpeedModifier);

            // Complete transaction
            currentCustomer.CompleteTransaction();

            // Play cash register sound
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySound(SoundType.CashRegister);
            }

            // Wait for customer to leave before processing next
            yield return new WaitForSeconds(0.5f);
        }

        currentCustomer = null;
        isProcessing = false;
    }

    public int GetQueueLength() {
        return customerQueue.Count;
    }

    public bool IsProcessing() {
        return currentCustomer != null;
    }

    /// <summary>
    /// Report items sold for objective tracking
    /// </summary>
    public void ReportItemSold(ItemDataSO itemData, int quantity) {
        if (itemData != null) {
            OnItemSold?.Invoke(itemData, quantity);
        }
    }

    /// <summary>
    /// Apply checkout speed upgrade (called by UpgradeManager)
    /// </summary>
    /// <param name="modifier">Speed modifier (e.g., 0.75 for 25% faster)</param>
    public void ApplySpeedUpgrade(float modifier) {
        checkoutSpeedModifier = modifier;
        Debug.Log($"Checkout speed upgraded! New modifier: {modifier}x (transaction time: {transactionTime * modifier}s)");
    }
}