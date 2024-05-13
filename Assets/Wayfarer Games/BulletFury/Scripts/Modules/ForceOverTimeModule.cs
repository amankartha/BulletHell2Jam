using System;
using BulletFury.Data;
using Unity.Mathematics;
using UnityEngine;

namespace BulletFury.Modules
{
    [Serializable, Tooltip("Settings for controlling bullet force over time.")]
    public class ForceOverTimeModule : BulletModule
    {
        public ForceSpace space = ForceSpace.World;
        public Vector3 scale = Vector3.one;
        public AnimationCurve forceOverTimeX;
        public AnimationCurve forceOverTimeY;
        public AnimationCurve forceOverTimeZ;

        public override void Execute(ref BulletContainer bullet, float deltaTime)
        {

            // if the force space is local, use the bullet's local axes 
            if (space == ForceSpace.Local)
            {
                bullet.Force += bullet.Right * forceOverTimeX.Evaluate(Mode == CurveUsage.Lifetime
                                    ? bullet.CurrentLifePercent
                                    : bullet.CurrentLifeSeconds % Time / Time) * scale.x +
                                bullet.Up * forceOverTimeY.Evaluate(Mode == CurveUsage.Lifetime
                                    ? bullet.CurrentLifePercent
                                    : bullet.CurrentLifeSeconds % Time / Time) * scale.y +
                                bullet.Forward * forceOverTimeZ.Evaluate(Mode == CurveUsage.Lifetime
                                    ? bullet.CurrentLifePercent
                                    : bullet.CurrentLifeSeconds % Time / Time) * scale.z;
            }
            else // if the force space is world, use the world axes
            {
                bullet.Force += new float3(
                    forceOverTimeX.Evaluate(Mode == CurveUsage.Lifetime
                        ? bullet.CurrentLifePercent
                        : bullet.CurrentLifeSeconds % Time / Time) * scale.x,
                    forceOverTimeY.Evaluate(Mode == CurveUsage.Lifetime
                        ? bullet.CurrentLifePercent
                        : bullet.CurrentLifeSeconds % Time / Time) * scale.y,
                    forceOverTimeZ.Evaluate(Mode == CurveUsage.Lifetime
                        ? bullet.CurrentLifePercent
                        : bullet.CurrentLifeSeconds % Time / Time) * scale.z
                );
            }
        }
    }
}