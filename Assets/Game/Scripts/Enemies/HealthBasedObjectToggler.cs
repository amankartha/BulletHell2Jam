using UnityEngine;

public class HealthBasedObjectToggler : MonoBehaviour
{
    // Reference to the script containing the health variables
    private Enemy healthScript;

    // Lists of GameObjects to enable and disable
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;

    void Start()
    {
        // Get the HealthScript component from the same GameObject
        healthScript = GetComponent<Enemy>();

        // Check if healthScript is found
        if (healthScript == null)
        {
            Debug.LogError("HealthScript component not found on the GameObject!");
        }
    }

    void Update()
    {
        // Check if healthScript is assigned
        if (healthScript != null)
        {
            // Calculate half of the maximum health
            float halfMaxHealth = healthScript.MaxHealth / 2;

            // Check if current health is equal to or less than half of the maximum health
            if (healthScript._health <= halfMaxHealth)
            {
                // Enable the specified GameObjects
                foreach (GameObject obj in objectsToEnable)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                    }
                }

                // Disable the specified GameObjects
                foreach (GameObject obj in objectsToDisable)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                    }
                }

                // Optionally, if you want to trigger this only once
                // you can disable this script after toggling the objects
                // enabled = false;
            }
        }
    }
}

// Assuming this is the health script attached to the same GameObject
public class HealthScript : MonoBehaviour
{
    public float currentHealth = 100f;
    public float MaxHealth = 100f;

    void Update()
    {
        // Example health decrement for testing
        if (currentHealth > 0)
        {
            currentHealth -= Time.deltaTime * 5; // Decrease health over time
        }
    }
}
