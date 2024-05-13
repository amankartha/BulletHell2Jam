using BulletFury.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace BulletFury
{
    [BurstCompile]
    public struct QuickSortJob : IJob 
    {
        public NativeArray<BulletContainer> entries;
        [ReadOnly] public int Count;
 
        public void Execute() {
            if (this.entries.Length > 0) {
                Quicksort(0, Count -1);
            }
        }
 
        private void Quicksort(int left, int right) {
            int i = left;
            int j = right;
            BulletContainer pivot = this.entries[(left + right) / 2];
 
            while (i <= j) {
                // Lesser
                while (Compare(this.entries[i], ref pivot) < 0) {
                    ++i;
                }
 
                // Greater
                while (Compare(this.entries[j], ref pivot) > 0) {
                    --j;
                }
 
                if (i <= j) {
                    // Swap
                    (this.entries[i], this.entries[j]) = (this.entries[j], this.entries[i]);

                    ++i;
                    --j;
                }
            }
 
            // Recurse
            if (left < j) {
                Quicksort(left, j);
            }
 
            if (i < right) {
                Quicksort(i, right);
            }
        }
 
        private int Compare(BulletContainer a, ref BulletContainer b) {
            return a.Dead.CompareTo(b.Dead);
        }
    }
    
    /// <summary>
    /// A C# job that moves all bullets based on their velocity and current force
    /// </summary>
#if !UNITY_EDITOR
    [BurstCompile]
#endif
    public struct BulletJob : IJobParallelFor
    {
        public NativeArray<BulletContainer> Bullets;
        [ReadOnly] public float DeltaTime;
        
        public void Execute(int index)
        {
            var container = Bullets[index];

            if (container.Dead == 1 || container.Waiting == 1 && container.CurrentLifeSeconds > container.TimeToWait)
                return;

            if (container.MovingToOrigin == 1)
            {
                container.MoveToOriginCurrentTime += DeltaTime;
                container.Position = math.lerp(container.MoveToOriginStartPosition, container.OriginPosition,
                    container.MoveToOriginCurrentTime / container.MoveToOriginTime);
                if(container.MoveToOriginCurrentTime >= container.MoveToOriginTime)
                    container.MovingToOrigin = 0;
                Bullets[index] = container;
                
                if (container.MovingToOrigin == 1)
                    return;
            }

            container.CurrentLifeSeconds += DeltaTime;
            if (container.CurrentLifeSeconds > container.Lifetime)
            {
                container.Dead = 1;
                container.EndOfLife = 1;

                Bullets[index] = container;
                return;
            }

            container.CurrentLifePercent = container.CurrentLifeSeconds / container.Lifetime;
            container.Position += container.Velocity * DeltaTime +
                                  container.Force * DeltaTime;
            
           
            container.Rotation =  math.normalize(container.Rotation);
            Bullets[index] = container;
        }
    }
}