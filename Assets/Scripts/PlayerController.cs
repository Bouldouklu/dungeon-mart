using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float interactionRange = 1.5f;

    private Vector2 moveInput;
    private Shelf nearestShelf;

    private void Update()
    {
        HandleMovement();
        FindNearestShelf();
        HandleInteraction();
    }

    private void HandleMovement()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.deltaTime;
        transform.position += movement;
    }

    private void FindNearestShelf()
    {
        Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);
        nearestShelf = null;
        float closestDistance = interactionRange;

        foreach (Shelf shelf in shelves)
        {
            float distance = Vector3.Distance(transform.position, shelf.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestShelf = shelf;
            }
        }
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E) && nearestShelf != null)
        {
            nearestShelf.RestockShelf(1);
        }
    }
}
