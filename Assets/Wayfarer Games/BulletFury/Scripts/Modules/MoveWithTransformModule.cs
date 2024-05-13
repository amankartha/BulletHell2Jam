using System;
using System.Collections.Generic;
using BulletFury.Data;
using Unity.Mathematics;
using UnityEngine;

namespace BulletFury.Modules
{
    [Serializable]
    public class MoveWithTransformModule : IBulletModule
    {
        [SerializeField] private Transform toFollow;

        private Dictionary<int, Vector3> _previousPosition = new ();
        
        public void Execute(ref BulletContainer container, float deltaTime)
        {
            if (toFollow == null) return;
            if (!_previousPosition.ContainsKey(container.Id))
                _previousPosition.Add(container.Id,  toFollow.position);
            
            container.Position += (float3)(toFollow.position - _previousPosition[container.Id]);
            _previousPosition[container.Id] = toFollow.position;
        }
    }
}