using System;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Data;
using Unity.Mathematics;
using UnityEngine;

namespace BulletFury.Modules
{
    public enum AimType
    {
        Instant,
        Linear,
        Slerp,
        SmoothDamp,
        Predicted,
        
    }

    [Serializable]
    public class RotateWithTransformModule : IBulletModule
    {
        [SerializeField] private Transform toFollow;
        private Dictionary<int, Vector3> _prevRotation = new ();
        public void Execute(ref BulletContainer container, float deltaTime)
        {
            if (toFollow == null) return;
            if (!_prevRotation.ContainsKey(container.Id))
                _prevRotation.Add(container.Id, toFollow.eulerAngles);
            
            var rotationDelta = Quaternion.Euler(toFollow.eulerAngles - _prevRotation[container.Id]);
            var position = toFollow.position;

            // First translate, then rotate
            container.Position = position + (rotationDelta * ((Vector3)container.Position - position));
            container.Rotation = rotationDelta * container.Rotation;

            _prevRotation[container.Id] = toFollow.eulerAngles;
        }
    }
}