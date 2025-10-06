using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private float spawnInterval = 10f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnCustomer();
            timer = 0f;
        }

        // Manual spawn with Space key for testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnCustomer();
        }
    }

    private void SpawnCustomer()
    {
        if (customerPrefab != null)
        {
            GameObject customerObj = Instantiate(customerPrefab, transform.position, Quaternion.identity);
            Customer customer = customerObj.GetComponent<Customer>();
            if (customer != null)
            {
                customer.Initialize();
                Debug.Log("Customer spawned!");
            }
        }
    }
}
