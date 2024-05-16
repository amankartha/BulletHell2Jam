using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundParent : MonoBehaviour
{
    public List<GameObject> objectsToRotate; // List of game objects to rotate
    public float radius = 5f; // Radius of the rotation
    public float rotationSpeed = 30f; // Speed of the rotation in degrees per second

    private Transform parentObject; // The parent object around which the game objects will rotate
    private List<Vector3> initialOffsets; // Store initial offsets relative to the parent

    void Start()
    {
        // Set the parent object to this game object's transform
        parentObject = transform;

        // Initialize the initial offsets list
        initialOffsets = new List<Vector3>();
        foreach (var obj in objectsToRotate)
        {
            Vector3 offset = obj.transform.position - parentObject.position;
            initialOffsets.Add(new Vector3(offset.x, offset.y, 0).normalized * radius);
        }
    }

    void Update()
    {
        // Rotate each object around the parent object
        for (int i = 0; i < objectsToRotate.Count; i++)
        {
            // Calculate the angle increment
            float angle = rotationSpeed * Time.time + (i * 360f / objectsToRotate.Count);

            // Calculate the new position
            float rad = Mathf.Deg2Rad * angle;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;

            // Set the position relative to the parent object
            objectsToRotate[i].transform.position = parentObject.position + offset;
        }
    }
}

