using System;
using BulletFury.Data;
using UnityEngine;

namespace BulletFury
{
    [Serializable]
    public class SubSpawnerData
    {
        public bool emitOnCollide = true;
        public bool emitOnLifeEnd = true;
        public bool inheritRotation = true;
        public bool inheritColor = true;
        
        public BulletSpawner spawner;

        public void Spawn(BulletContainer parent)
        {
            if (inheritColor)
                spawner.Main.StartColor = parent.Color;
            
            if (inheritRotation)
                spawner.Spawn(parent.Position, parent.Up, Time.deltaTime);
            else
                spawner.Spawn(parent.Position, spawner.transform.up, Time.deltaTime);
        }
    }
}