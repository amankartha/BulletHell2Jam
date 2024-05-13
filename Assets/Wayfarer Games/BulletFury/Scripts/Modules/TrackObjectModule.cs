using System;
using BulletFury.Data;
using UnityEngine;

namespace BulletFury.Modules
{
    [Serializable]
    public class TrackObjectModule : BulletModule
    {
        [field: SerializeField] public Transform ToTrack { get; private set;}
        [field: SerializeField] public float TurnSpeed { get; private set;}
        
        [SerializeField] private AnimationCurve turnSpeedOverTime = AnimationCurve.Constant(0, 1, 1);
        
        public override void Execute(ref BulletContainer bullet, float deltaTime)
        {
            if (bullet.Dead == 1 || ToTrack == null) return;

            var target = ToTrack.position - (Vector3)bullet.Position;
            var turn = TurnSpeed * turnSpeedOverTime.Evaluate(Mode == CurveUsage.Lifetime
                ? bullet.CurrentLifePercent
                : bullet.CurrentLifeSeconds % Time / Time);

            bullet.Up = bullet.Rotation * Vector3.up;
            bullet.Up = Vector3.RotateTowards(bullet.Up, target.normalized, turn * deltaTime, 0.0f);
            bullet.Forward = bullet.Rotation * Vector3.forward;
            bullet.Right = bullet.Rotation * Vector3.right;

            bullet.Rotation = Quaternion.LookRotation(bullet.Forward, bullet.Up);
        }
    }
}