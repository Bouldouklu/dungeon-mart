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
                Destroy(gameObject);
                yield break;
            }

            // Move to checkout
            GameObject checkout = GameObject.FindGameObjectWithTag("Checkout");
            if (checkout != null)
            {
                Debug.Log("Found checkout at: " + checkout.transform.position);
                Vector3 checkoutPos = checkout.transform.position + new Vector3(0, -1f, 0);
                MoveToPosition(checkoutPos);
                while (isMoving)
                {
                    yield return null;
                }

                // Complete purchase
                yield return new WaitForSeconds(1f);
                if (carriedItem != null)
                {
                    GameManager.Instance.AddMoney(carriedItem.GetSellPrice());
                    Destroy(carriedItem.gameObject);
                }
            }
        }

        // Leave
        Destroy(gameObject);
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
