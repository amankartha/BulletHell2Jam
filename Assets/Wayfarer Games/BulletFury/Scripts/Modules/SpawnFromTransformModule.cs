using System;
using BulletFury.Data;
using UnityEngine;

namespace BulletFury.Modules
{
    [Serializable]
    public class SpawnFromTransformModule : IBulletInitModule
    {
        [SerializeField] private float secondsToOriginalPosition;
        [SerializeField] private Transform spawnPosition;
        
        public void Execute(ref BulletContainer container)
        {
            if (spawnPosition == null) return;
            container.OriginPosition = container.Position;
            container.Position = spawnPosition.position;
            container.MovingToOrigin = 1;
            container.MoveToOriginCurrentTime = 0;
            container.MoveToOriginTime = secondsToOriginalPosition;
            container.MoveToOriginStartPosition = container.Position;
        }
    }
}