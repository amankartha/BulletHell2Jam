using System.Threading.Tasks;
using BulletFury.Data;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BulletFury
{
    public static class UpdateBullets
    {
        public static void Update(NativeArray<BulletContainer> bullets,
            BulletMainData visuals, float deltaTime, bool active, int numBullets, Transform transform, Vector3 previousPosition, Vector3 prevRotation)
        {
            for (int i = numBullets - 1; i >= 0; --i)
            {
                var bullet = bullets[i];
                // if the bullet is dead or waiting, don't do anything
                if (bullet.Dead == 1 || (bullet.Waiting == 1 && bullet.CurrentLifeSeconds > bullet.TimeToWait))
                    continue;
                
                if (!active)
                    bullet.Dead = 1;

                bullet.ColliderSize = bullet.CurrentSize * visuals.ColliderSize / 2f;

                if (visuals.UseRotationForDirection)
                    bullet.Velocity = bullet.Rotation * Vector3.up;
                else
                    bullet.Velocity = bullet.Direction * Vector3.up;
                
                bullet.Velocity *= bullet.CurrentSpeed;
                
                if (visuals.MoveWithTransform)
                    bullet.Position += (float3)(transform.position - previousPosition);

                if (visuals.RotateWithTransform)
                {
                    var rotationDelta = Quaternion.Euler(transform.eulerAngles - prevRotation);
                    bullet.Position = (rotationDelta) * (bullet.Position - (float3) transform.position) + transform.position;
                    bullet.Rotation = rotationDelta * bullet.Rotation;
                }

                bullets[i] = bullet;
            }

            // create a new job
            var bulletJob = new BulletJob
            {
                DeltaTime = deltaTime,
                Bullets = bullets,
            };

            // start the job
            var handle = bulletJob.Schedule(bullets.Length, 256);

            //if (Application.isPlaying)
              //  await Awaitable.FixedUpdateAsync();

            // make sure the job is finished
            handle.Complete();
        }
    }
}