using System;
using BulletFury.Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace BulletFury.Modules
{
    [Serializable]
    public class BulletColorOverTimeModule : BulletModule
    {
        [GradientUsage(true)] [Tooltip("The gradient to apply to the bullet over time")]
        public Gradient colorOverTime;

        public override void Execute(ref BulletContainer bullet, float deltaTime)
        {
            bullet.Color = bullet.StartColor * colorOverTime.Evaluate(Mode == CurveUsage.Lifetime
                ? bullet.CurrentLifePercent
                : bullet.CurrentLifeSeconds % Time / Time);
        }
    }
}