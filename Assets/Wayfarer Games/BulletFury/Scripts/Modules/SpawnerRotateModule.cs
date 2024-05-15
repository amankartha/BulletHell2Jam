using System;
using UnityEngine;

namespace BulletFury.Modules
{
    [Serializable]
    public class SpawnerRotateModule : IBulletSpawnModule
    {
        [SerializeField] private float angularSpeed;

        [NonSerialized] private float _currentAngle;
        
        public void Execute(ref Vector3 _, ref Quaternion rotation, float deltaTime)
        {
            _currentAngle += angularSpeed * deltaTime;
            rotation = Quaternion.AngleAxis(_currentAngle, Vector3.forward) * rotation;
            _currentAngle = (_currentAngle + 360) % 360f;
        }
    }
}