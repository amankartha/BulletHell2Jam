using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCannonArm : MonoBehaviour
{
    public Transform player; // Reference to the player's Transform component
    public float rotationSpeed = 5f; // Speed of rotation
    public float minAngle = -45f; // Minimum angle the arm can rotate
    public float maxAngle = 45f; // Maximum angle the arm can rotate

    void Update()
    {
        if (player != null)
        {
            // Calculate direction vector from the arm to the player
            Vector3 direction = player.position - transform.position;

            // Calculate rotation angle in degrees
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            // Clamp the angle within the specified range
            targetAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);

            // Smoothly rotate the arm towards the player's position
            float currentAngle = transform.eulerAngles.z;
            currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);

            // Apply the rotation to the arm
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
    }
}
