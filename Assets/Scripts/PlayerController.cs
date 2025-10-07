using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float interactionRange = 1.5f;

    private Rigidbody2D rb2d;
    private Vector2 moveInput;
    private Shelf nearestShelf;

    private void Awake() {
        rb2d = GetComponent<Rigidbody2D>();

        // Enforce proper Rigidbody2D settings for collision detection
        rb2d.bodyType = RigidbodyType2D.Dynamic;
        rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb2d.gravityScale = 0f;
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;

        Debug.Log("PlayerController: Rigidbody2D configured for collision detection");
    }

    private void Update() {
        HandleMovement();
        FindNearestShelf();
        HandleInteraction();
    }

    private void HandleMovement() {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        // Use velocity for Dynamic Rigidbody2D
        rb2d.linearVelocity = moveInput * moveSpeed;
    }

    private void FindNearestShelf() {
        Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);
        nearestShelf = null;
        float closestDistance = interactionRange;

        foreach (Shelf shelf in shelves) {
            float distance = Vector3.Distance(transform.position, shelf.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                nearestShelf = shelf;
            }
        }
    }

    private void HandleInteraction() {
        if (Input.GetKeyDown(KeyCode.E) && nearestShelf != null) {
            bool success = nearestShelf.RestockShelf(1);
            if (!success) {
                Debug.LogWarning("Cannot restock shelf - check inventory or shelf capacity");
            }
        }

        // Debug key to add test inventory
        if (Input.GetKeyDown(KeyCode.I)) {
            InventoryManager.Instance.AddDebugInventory();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
    }
}