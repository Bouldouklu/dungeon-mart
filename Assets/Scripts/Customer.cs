using System.Collections;
using UnityEngine;

public class Customer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Transform itemCarryPoint;

    private Shelf targetShelf;
    private Item carriedItem;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private bool waitingForCheckout = false;
    private CheckoutCounter checkoutCounter;

    public bool IsMoving => isMoving;

    public void Initialize()
    {
        StartCoroutine(ShoppingRoutine());
    }

    private IEnumerator ShoppingRoutine()
    {
        // Find a shelf
        yield return new WaitForSeconds(0.5f);

        Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);
        if (shelves.Length > 0)
        {
            targetShelf = shelves[Random.Range(0, shelves.Length)];

            // Move to shelf
            Debug.Log("Moving to shelf at: " + targetShelf.transform.position);
            Vector3 shelfPos = targetShelf.transform.position + new Vector3(0, -1f, 0);
            MoveToPosition(shelfPos);
            while (isMoving)
            {
                yield return null;
            }

            // Take item
            yield return new WaitForSeconds(1f);
            if (targetShelf != null && !targetShelf.IsEmpty)
            {
                carriedItem = targetShelf.TakeItem();
                if (carriedItem != null && itemCarryPoint != null)
                {
                    carriedItem.transform.SetParent(itemCarryPoint);
                    carriedItem.transform.localPosition = Vector3.zero;
                }
            }

            // If didn't get an item, leave disappointed
            if (carriedItem == null)
            {
                Debug.Log("Customer leaving - no items available!");
                DayManager.Instance?.RecordCustomerLeft();
                NotifySpawnerAndLeave();
                yield break;
            }

            // Find checkout counter and join queue
            checkoutCounter = FindFirstObjectByType<CheckoutCounter>();
            if (checkoutCounter != null)
            {
                Debug.Log("Joining checkout queue");
                waitingForCheckout = true;
                checkoutCounter.JoinQueue(this);

                // Wait for our turn at checkout
                while (waitingForCheckout)
                {
                    yield return null;
                }
            }
            else
            {
                Debug.LogWarning("No CheckoutCounter found in scene!");
            }
        }

        // Leave store
        NotifySpawnerAndLeave();
    }

    private void NotifySpawnerAndLeave()
    {
        if (CustomerSpawner.Instance != null)
        {
            CustomerSpawner.Instance.OnCustomerLeft();
        }
        Destroy(gameObject);
    }

    public void ApproachCheckout(Vector3 checkoutPosition)
    {
        Debug.Log("Approaching checkout counter");
        MoveToPosition(checkoutPosition);
    }

    public void CompleteTransaction()
    {
        Debug.Log("Completing transaction");

        // Process payment
        if (carriedItem != null)
        {
            int salePrice = carriedItem.GetSellPrice();
            GameManager.Instance.AddMoney(salePrice);
            DayManager.Instance?.RecordCustomerSale(salePrice);
            Destroy(carriedItem.gameObject);
            carriedItem = null;
        }

        // Signal we're done waiting
        waitingForCheckout = false;
    }

    private void MoveToPosition(Vector3 target)
    {
        targetPosition = target;
        isMoving = true;
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
            }
        }
    }
}
