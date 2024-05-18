using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateChildrenAfterDelay : MonoBehaviour
{
    // Time in seconds to wait before activating the child GameObjects
    public float delayInSeconds;

    // List to hold the child GameObjects
    private List<GameObject> childObjects;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the list of child objects
        childObjects = new List<GameObject>();

        // Get all child GameObjects and add them to the list
        foreach (Transform child in transform)
        {
            childObjects.Add(child.gameObject);
        }

        // Start the coroutine to activate the GameObjects after the delay
        StartCoroutine(ActivateObjectsAfterDelay());
    }

    // Coroutine to handle the delayed activation
    private IEnumerator ActivateObjectsAfterDelay()
    {
        // Wait for the specified amount of seconds
        yield return new WaitForSeconds(delayInSeconds);

        // Activate all the child GameObjects
        foreach (GameObject child in childObjects)
        {
            child.SetActive(true);
        }
    }
}
