using System;
using BulletFury.Data;
using UnityEngine;

namespace BulletFury.Modules
{
    [Serializable]
    public class WaitToContinueModule : IBulletInitModule
    {
        [SerializeField] private float timeToPlayBeforeWaiting;
        public void Execute(ref BulletContainer container)
        {
            container.Waiting = 1;
            container.TimeToWait = timeToPlayBeforeWaiting;
        }
    }
}