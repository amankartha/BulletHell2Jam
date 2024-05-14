using System;
using BulletFury.Data;
using UnityEngine;

namespace BulletFury.Modules
{
    [Serializable]
    public class BulletSizeOverTimeModule : BulletModule
    {
        [Tooltip("The size curve to apply to the bullet over time")]
        public AnimationCurve sizeOverTime = AnimationCurve.Constant(0, 1, 1);

        public override void Execute(ref BulletContainer bullet, float deltaTime)
        {
            bullet.CurrentSize = bullet.StartSize * sizeOverTime.Evaluate(Mode == CurveUsage.Lifetime
                ? bullet.CurrentLifePercent
                : bullet.CurrentLifeSeconds % Time / Time);
        }
    }
}