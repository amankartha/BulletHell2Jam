using System;
using UnityEngine;
using Wayfarer_Games.Common;

namespace BulletFury.Modules
{
    public enum AimType
    {
        Instant, Linear, Slerp, SmoothDamp, Predicted
    }
    [Serializable]
    public class AimedShotModule : IBulletSpawnModule
    {
        [SerializeField] private Transform thisTransform;
        [SerializeField] private Transform target;
        [SerializeField] private AimType type;

        [Header("Linear")]
        [SerializeField, Tooltip("How much can the rotation change, in degrees, every time bullets are spawned?")] 
        private float maxChangePerFrame = 10f;

        [Header("Slerp")]
        [SerializeField] private float lerpSpeed = 0.1f;

        [Header("SmoothDamp")] 
        [SerializeField]
        private float time = 0.5f;
        private Quaternion _velocity;

        [Header("Predicted")] 
        [SerializeField] private float lookAheadTime = 1f;
        private Vector3? _previousPosition;
        private Quaternion _previousRotation;
        private Vector3? _cachedVelocity;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        public void Execute(ref Vector3 _, ref Quaternion rotation, float deltaTime)
        {
            if (target == null || thisTransform == null) return; // Safety check
            Vector3 directionToTarget = (target.position - thisTransform.position).normalized;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90); // Calculate target rotation based on direction

            switch (type)
            {
                case AimType.Instant:
                    rotation = targetRotation; // Instant rotation to face the target
                    break;

                case AimType.Linear:
                    rotation = Quaternion.RotateTowards(_previousRotation, targetRotation, maxChangePerFrame * deltaTime); // Linear rotation with max angle change
                    break;

                case AimType.Slerp:
                    float t = 1f - Mathf.Pow(0.5f, deltaTime * lerpSpeed); // Framerate-independent t value
                    rotation = Quaternion.Slerp(_previousRotation, targetRotation, t); // Smooth slerp rotation
                    break;

                case AimType.SmoothDamp:
                    rotation = QuaternionUtil.SmoothDamp(_previousRotation, targetRotation, ref _velocity, time, deltaTime); // Use your custom SmoothDamp implementation
                    break;

                case AimType.Predicted:
                    if (_previousPosition == null)
                    {
                        _previousPosition = target.position;
                        return;
                    }
                    
                    if (!Mathf.Approximately(deltaTime, 0))
                        _cachedVelocity = (target.position - _previousPosition.Value) / deltaTime;

                    if (_cachedVelocity == null) return;

                    Vector3 targetVelocity = _cachedVelocity.Value; 
                    Vector3 predictedPosition = target.position + targetVelocity * lookAheadTime;

                    directionToTarget = (predictedPosition - thisTransform.position).normalized;
                    targetRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90);

                    rotation = targetRotation; // Aim at the predicted position instantly

                    _previousPosition = target.position; // Update previous position for the next frame
                    break;
            }

            _previousRotation = rotation;
        }
    }
}