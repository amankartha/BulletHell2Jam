using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCannonArm : MonoBehaviour
{
    public Transform player; // Reference to the player's Transform component
    public float rotationSpeed = 5f; // Speed of rotation
    public float minAngle = -45f; // Minimum angle relative to starting direction
    public float maxAngle = 45f; // Maximum angle relative to starting direction

    private Quaternion initialRotation;

    private void Start()
    {
        player = GameManager.Instance.MAINPLAYERGAMEOBJECT.transform;
        // Store the initial rotation of the arm
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (player != null)
        {
            // Calculate direction vector from the arm to the player
            Vector3 direction = player.position - transform.position;

            // Calculate the rotation angle in degrees relative to initial rotation
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            // Calculate the angle difference relative to the initial rotation
            float angleDifference = Mathf.DeltaAngle(initialRotation.eulerAngles.z, targetAngle);

            // Clamp the angle difference within the specified range
            float clampedAngleDifference = Mathf.Clamp(angleDifference, minAngle, maxAngle);

            // Calculate the final target angle based on the initial rotation and clamped angle difference
            float finalTargetAngle = initialRotation.eulerAngles.z + clampedAngleDifference;

            // Smoothly rotate the arm towards the player's position
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, finalTargetAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
