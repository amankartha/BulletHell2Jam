using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulletFury.Data;
using Common;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;

namespace BulletFury
{
    public interface IBulletHitHandler
    {
        public void Hit(BulletContainer bullet);
    }
    
    public interface IBaseBulletModule { }

    public interface IBulletModule : IBaseBulletModule
    {
        public void Execute(ref BulletContainer container, float deltaTime);
    }
    
    public interface IBulletInitModule : IBaseBulletModule
    {
        public void Execute(ref BulletContainer container);
    }

    public interface IBulletSpawnModule : IBaseBulletModule
    {
        public void Execute(ref Vector3 position, ref Quaternion rotation, float deltaTime);
    }
    
    [DefaultExecutionOrder(-1000)]
    public class BulletSpawner : MonoBehaviour
    {
        private const int MaxBullets = 10000;
        [SerializeField] private SharedRenderData renderData;
        [SerializeField] private BulletMainData main;
        [SerializeField] private SpawnShapeData spawnShapeData;
        
        [SerializeField] private BurstData burstData;
        [SerializeField] private SubSpawnerData[] subSpawners;
        
        #if SERIALIZEREFERENCE_EXTENSIONS
        [SerializeReference, SubclassSelector, Obsolete]
        #endif
        private List<IBulletModule> bulletModules = new ();
        
        #if SERIALIZEREFERENCE_EXTENSIONS
        [SerializeReference, SubclassSelector, Obsolete]
        #endif
        private List<IBulletInitModule> bulletInitModules = new ();
        
        #if SERIALIZEREFERENCE_EXTENSIONS
        [SerializeReference, SubclassSelector, Obsolete]
        #endif
        private List<IBulletSpawnModule> spawnModules = new ();
        
        #if SERIALIZEREFERENCE_EXTENSIONS
        [SerializeReference, SubclassSelector]
        #endif
        private List<IBaseBulletModule> allModules = new ();

        private bool _isStopped = false;
        
        // Unity Event that fires when a bullet reaches end-of-life, can be set in the inspector like a button 
        // ReSharper disable once InconsistentNaming
        [SerializeField] private BulletDiedEvent OnBulletDied;
        public event Action<BulletContainer, bool> OnBulletDiedEvent;

        [SerializeField] private BulletDiedEvent OnBulletCancelled;
        public event Action<BulletContainer> OnBulletCancelledEvent;
        
        
        // Unity Event that fires when a bullet is spawned, can be set in the inspector like a button 
        // ReSharper disable once InconsistentNaming
        [SerializeField] private BulletSpawnedEvent OnBulletSpawned;
        public event Action<int, BulletContainer> OnBulletSpawnedEvent;
        
        [SerializeField] private UnityEvent OnWeaponFired;
        public event Action OnWeaponFiredEvent;
        
        public SharedRenderData RenderData => renderData;
        public BulletMainData Main => main;
        public SpawnShapeData SpawnShapeData => spawnShapeData;
        public BurstData BurstData => burstData;
        public SubSpawnerData[] SubSpawners => subSpawners;

        private bool _enabled = true;
        private float _currentTime = float.MaxValue;
        private Vector3 _previousPos, _previousRot;
        private float _currentRotationAngle;
        private bool _hasSpawnedSinceEnable = false;
        private int _bulletCount;
        private bool _bulletsFree = true;
        
        private Collider2D[] _hit = new Collider2D[4];
        private ContactFilter2D _filter;
        private HashSet<int> _bulletsToKill = new();
        private int _bulletMaxCount = 0;

        private NativeArray<BulletContainer> _bullets;
        private (BulletRenderData renderData, Camera cam)? _queuedRenderData;

        private Squirrel3 _rnd = new Squirrel3();

        private bool _disposed = false;
        public bool Disposed => _disposed;
        public struct RenderQueueData
        {
            public BulletRenderData RenderData;
            public int Count;
            public NativeArray<BulletContainer> Bullets;
            public BulletSpawner Spawner;
        }
        private static SortedList<float, RenderQueueData> _renderQueue = new();
        public static SortedList<float, RenderQueueData> RenderQueue => _renderQueue;

        public void Start()
        {
            _bullets = new NativeArray<BulletContainer>(burstData.maxActiveBullets == 0 ? MaxBullets : burstData.maxActiveBullets, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _disposed = false;
            for (int i = 0; i < _bullets.Length; i++)
            {
                _bullets[i] = new BulletContainer
                {
                    Id = i,
                    Dead = 1
                };
            }

            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = Physics2D.GetLayerCollisionMask(gameObject.layer),
                useTriggers = true
            };

        }

        private void OnEnable()
        {
            if (burstData.delay > 0)
                _currentTime = Main.FireRate - burstData.delay;
            _isStopped = !main.PlayOnEnable;
        }

        private void OnValidate()
        {
            if (bulletInitModules.Any())
            {
                foreach (var mod in bulletInitModules)
                    allModules.Add(mod);
                bulletInitModules.Clear();
            }

            if (spawnModules.Any())
            {
                foreach (var mod in spawnModules)
                    allModules.Add(mod);
                spawnModules.Clear();
            }

            if (bulletModules.Any())
            {
                foreach (var mod in bulletModules)
                    allModules.Add(mod);
                bulletModules.Clear();
            }
        }

        public void SetPreset(BulletSpawnerPreset preset)
        {
            if (preset.UseMain)
                main = preset.Main;
            if (preset.UseShape)
                spawnShapeData = preset.ShapeData;
            if (preset.UseBurstData)
                burstData = preset.BurstData;
            if (preset.UseSubSpawners)
                subSpawners = preset.SubSpawners;
            if (preset.UseModules)
                allModules = preset.BulletModules;
        }
        
        public void OnDestroy()
        {
            if (_disposed) return;
            _bullets.Dispose();
            _disposed = true;
        }

        public void Stop()
        {
            _isStopped = true;
        }

        public void Play()
        {
            _isStopped = false;
        }

        private void FixedUpdate()
        {
            UpdateAllBullets(renderData.Data.Camera, Time.fixedDeltaTime);
        }

        public T GetModule<T>() where T : IBaseBulletModule
        {
            return (T) allModules.First(m => m is T);
        }

        public List<T> GetModulesOfType<T>() where T : IBaseBulletModule
        {
            return allModules
                .OfType<T>()
                .ToList();
        }

        public void RenderBulletsNow()
        {
            if (_queuedRenderData == null || _disposed) return;
            
            BulletRenderer.Render(_queuedRenderData.Value.renderData, _bullets, _bulletCount,
                _queuedRenderData.Value.cam);
        }
        
        private void Update()
        { 
            if (_queuedRenderData == null || _disposed) return;
            float priority = -_queuedRenderData.Value.renderData.Priority;
            while (_renderQueue.ContainsKey(priority))
                priority += 0.01f;
            
            _renderQueue.Add(priority, new RenderQueueData
            {
                Bullets = _bullets,
                Count = _bulletCount,
                RenderData = _queuedRenderData.Value.renderData,
                Spawner = this
            });
        }

        public void UpdateAllBullets(Camera cam, float? dt = null)
        {
            if (!_bulletsFree || this == null || renderData.Data.Texture == null) return;
            var deltaTime = dt ?? Time.deltaTime;
            
            _currentRotationAngle += spawnShapeData.rotateSpeed * deltaTime;
            // increment the current timer
            _currentTime += deltaTime;
            
            if (Main.FireMode == FireMode.Automatic && enabled && !_isStopped)
                Spawn(transform, deltaTime);
            if (_bulletCount == 0) return;
            _bulletMaxCount = Mathf.Max(_bulletMaxCount, _bulletCount);
            
            _bulletsFree = false;

            UpdateBullets.Update(_bullets,
                main, 
                deltaTime,
                _enabled, 
                _bulletCount,
                transform,
                _previousPos,
                _previousRot);
            _bulletsFree = true;
            

            foreach (var module in allModules)
            {
                if (module is not IBulletModule mod) continue;
                for (int i = _bulletCount - 1; i >= 0; --i)
                {
                    var bullet = _bullets[i];
                    mod.Execute(ref bullet, deltaTime);
                    _bullets[i] = bullet;
                }
            }
            
            HandleCollisions();
            
            _previousPos = transform.position;
            _previousRot = transform.eulerAngles;

            for (int i = _bulletCount - 1; i >= 0; --i)
            {
                var bullet = _bullets[i];
                if (_bullets[i].EndOfLife == 1)
                {
                    OnBulletDied?.Invoke(_bullets[i].Id, _bullets[i], true);
                    OnBulletDiedEvent?.Invoke(_bullets[i], true);
                    bullet.EndOfLife = 0;
                    --_bulletCount;
                    foreach (var subSpawner in subSpawners)
                    {
                        if (subSpawner.emitOnLifeEnd)
                            subSpawner.Spawn(_bullets[i]);
                    }
                }

                if (_bulletsToKill.Contains(i))
                {
                    bullet.Dead = 1;
                    --_bulletCount;
                    OnBulletDied?.Invoke(_bullets[i].Id, _bullets[i], false);
                    OnBulletDiedEvent?.Invoke(_bullets[i], false);
                    
                    foreach (var subSpawner in subSpawners)
                    {
                        if (subSpawner.emitOnCollide)
                            subSpawner.Spawn(_bullets[i]);
                    }
                }

                _bullets[i] = bullet;
            }
            
            _bulletsToKill.Clear();
            
            var quickSort = new QuickSortJob
            {
                entries = _bullets,
                Count = _bulletMaxCount
            };
            
            if (Application.isPlaying)
            {
                // start the job
                quickSort
                    .Schedule()
                    .Complete();
            }

            _queuedRenderData = (renderData, cam);
        }

        private void HandleCollisions()
        {
            var shouldKill = false;
            for (int i = _bulletCount - 1; i >= 0; --i)
            {
                if (_bullets[i].UseCapsule == 0)
                {
                    int numHit =
                        Physics2D.OverlapCircle((Vector3)_bullets[i].Position, _bullets[i].ColliderSize, _filter, _hit);
                    if (numHit > 0)
                    {
                        
                        for (int j = 0; j < numHit; ++j)
                        {
                            var hit = _hit[j];
                            if (!hit.isTrigger) shouldKill = true;
                            if (Application.isPlaying)
                            {
                                var bulletHandlers = hit.GetComponentsInChildren<IBulletHitHandler>();
                                foreach (var handler in bulletHandlers)
                                    handler.Hit(_bullets[i]);
                            }
                        }
                        if (shouldKill)
                            _bulletsToKill.Add(i);
                    }
                }
                else
                {
                    int numHit = Physics2D.OverlapCapsule((Vector3)_bullets[i].Position, new Vector2(_bullets[i].ColliderSize, _bullets[i].CapsuleLength), CapsuleDirection2D.Vertical, _bullets[i].Rotation.eulerAngles.z, _filter, _hit);
                    if (numHit > 0)
                    {
                        for (int j = 0; j < numHit; ++j)
                        {
                            var hit = _hit[j];
                            if (!hit.isTrigger) shouldKill = true;
                            if (Application.isPlaying)
                            {
                                var bulletHandlers = hit.GetComponentsInChildren<IBulletHitHandler>();
                                foreach (var handler in bulletHandlers)
                                    handler.Hit(_bullets[i]);
                            }
                        }
                        if (shouldKill)
                            _bulletsToKill.Add(i);
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_bulletCount == 0 || _disposed) return;
            #if UNITY_EDITOR
            if (UnityEditor.Selection.activeGameObject != gameObject) return;
            #endif
            for (int i = _bulletCount - 1; i >= 0; --i)
            {
                if (_bullets[i].Dead == 1) continue;
                if (_bullets[i].UseCapsule == 0)
                    Gizmos.DrawWireSphere(_bullets[i].Position, _bullets[i].ColliderSize);
                else
                {
                    Gizmos.DrawWireSphere(_bullets[i].Position + _bullets[i].Up * _bullets[i].CurrentSize * _bullets[i].CapsuleLength * 0.5f, _bullets[i].ColliderSize);
                    Gizmos.DrawWireSphere(_bullets[i].Position - _bullets[i].Up * _bullets[i].CurrentSize * _bullets[i].CapsuleLength * 0.5f, _bullets[i].ColliderSize);
                }
                //Debug.Log(_bullets[i].Position);
            }
        }

        public bool CheckBulletsRemaining()
        {
            if (burstData.maxActiveBullets != 0)
                return _bulletCount < burstData.maxActiveBullets - 1;
            
            return _bulletCount < MaxBullets - 1;

        }

        public async void Spawn(Vector3 position, Vector3 up, float deltaTime)
        {
            while (!_bulletsFree) await Task.Yield();
            if (_disposed) return;
            var hasBulletsLeft = CheckBulletsRemaining();
            
            // don't spawn a bullet if we haven't reached the correct fire rate
            if (_currentTime < Main.FireRate || !hasBulletsLeft|| !_enabled)
                return;
            // reset the current time
            _currentTime = 0;

            if (!gameObject.activeInHierarchy) return;
            
            if (!_hasSpawnedSinceEnable)
            {
                if (Mathf.Approximately(burstData.delay, 0))
                    _currentTime = Main.FireRate;
                //else
                  //  _currentTime = Main.FireRate - burstData.delay;
                _hasSpawnedSinceEnable = true;
            }
            
            OnWeaponFired?.Invoke();
            OnWeaponFiredEvent?.Invoke();
            // keep a list of positions and rotations, so we can update the bullets all at once
            var positions = new List<Vector3>();
            var rotations = new List<Quaternion>();
            for (int burstNum = 0; burstNum < burstData.burstCount; ++burstNum)
            {
                if (_disposed) return;
                // make sure the positions and rotations are clear before doing anything
                positions.Clear();
                rotations.Clear();
                int idx = 0;
                // spawn the bullets
                spawnShapeData.Spawn( (point, dir) =>
                {
                    var pos = (Vector3) point;

                    var extraRotation = Quaternion.LookRotation(Vector3.forward, up);

                    var fireRate = idx == 0 ? Main.FireRate : 0;
                    if (!burstData.burstsUpdatePositionEveryBullet)
                        fireRate = burstNum == 0 ? fireRate : 0;
                    
                    foreach (var module in spawnModules)
                        module?.Execute(ref pos, ref extraRotation, fireRate);
                    
                    foreach (var module in allModules)
                        if (module is IBulletSpawnModule mod)
                            mod.Execute(ref pos, ref extraRotation, fireRate);
                    
                    // rotate dir by this new rotation
                    Vector3 rotatedDir = extraRotation * dir;
                    Quaternion rotation = Quaternion.LookRotation(Vector3.forward, rotatedDir);
                    
                    
                    Vector3 spawnPosition = position + extraRotation * pos;

                    rotations.Add(rotation);
                    positions.Add(spawnPosition);
                    ++idx;
                }, Squirrel3.Instance);
                
                // for every bullet we found
                for (int i = 0; i < positions.Count; i++)
                {
                    // create a new container that isn't dead, at the position and rotation we found with the spawner
                    var newContainer = new BulletContainer
                    {
                        Dead = 0,
                        Position = positions[i],
                        Rotation = rotations[i],
                        Direction = rotations[i]
                    }; 
                    
                    var j = 0;
                    // find a bullet that isn't alive and replace it with this one
                    for (j = 0; j < _bullets.Length; ++j)
                    {
                        if (_bullets[j].Dead == 0) continue;
                        newContainer.Id = j;
                        _bullets[j] = newContainer;
                        break;
                    }
                    if ((burstData.maxActiveBullets == 0 && j >= _bullets.Length) || (burstData.maxActiveBullets > 0 && j >= burstData.maxActiveBullets))
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning($"Tried to spawn too many bullets on manager {name}, didn't spawn one.");
                        #endif
                        return;
                    }
                    // initialise the bullet
                    var bullet = _bullets[j];
                    
                    bullet.Damage = main.Damage;
                    bullet.Lifetime = main.Lifetime;
                    bullet.Speed = main.Speed;
                    bullet.CurrentSpeed = bullet.Speed;
                    bullet.AngularVelocity = 0f;
                    bullet.StartSize = main.StartSize.GetValue(_rnd);
                    bullet.CurrentSize = bullet.StartSize;
                    bullet.StartColor = main.StartColor;
                    bullet.Color = bullet.StartColor;
                    bullet.ColliderSize = bullet.CurrentSize * main.ColliderSize / 2f;
                    bullet.UseCapsule = main.ColliderType == ColliderType.Capsule ? (byte) 1 : (byte) 0;
                    bullet.CapsuleLength = main.CapsuleLength;
                    bullet.MovingToOrigin = 0;

                    foreach (var module in allModules)
                    {
                        if (module is IBulletInitModule initMod)
                            initMod.Execute(ref bullet);
                        if (module is IBulletModule bulletMod)
                            bulletMod.Execute(ref bullet, deltaTime);
                    }
                    
                    bullet.Speed += burstNum * burstData.stackSpeedIncrease;
                    bullet.CurrentSpeed += burstNum * burstData.stackSpeedIncrease;
                    _bullets[j] = bullet;
                    
                    ++_bulletCount;
                    OnBulletSpawned?.Invoke(j, _bullets[j]);
                    OnBulletSpawnedEvent?.Invoke(j, _bullets[j]);
                }
                
                
                #if UNITY_2023_1_OR_NEWER
                // wait a little bit before doing the next burst
                await Awaitable.WaitForSecondsAsync(burstData.burstDelay);
                #else
                var timer = burstData.burstDelay;
                while (timer >= 0)
                {
                    timer -= Time.deltaTime;
                    await Task.Yield();
                }
                #endif
            }
        }

        public async void Spawn(Transform obj, float deltaTime)
        {
            while (!_bulletsFree) await Task.Yield();
            if (_disposed) return;
            var hasBulletsLeft = CheckBulletsRemaining();
            
            // don't spawn a bullet if we haven't reached the correct fire rate
            if (_currentTime < Main.FireRate || !hasBulletsLeft|| !_enabled)
                return;
            // reset the current time
            _currentTime = 0;

            if (!gameObject.activeInHierarchy) return;
            
            if (!_hasSpawnedSinceEnable)
            {
                if (Mathf.Approximately(burstData.delay, 0))
                    _currentTime = Main.FireRate;
                //else
                   // _currentTime = Main.FireRate - burstData.delay;
                _hasSpawnedSinceEnable = true;
            }
            
            OnWeaponFired?.Invoke();
            OnWeaponFiredEvent?.Invoke();
            // keep a list of positions and rotations, so we can update the bullets all at once
            var positions = new List<Vector3>();
            var rotations = new List<Quaternion>();
            for (int burstNum = 0; burstNum < burstData.burstCount; ++burstNum)
            {
                if (_disposed) return;
                // make sure the positions and rotations are clear before doing anything
                positions.Clear();
                rotations.Clear();

                int idx = 0;
                // spawn the bullets
                spawnShapeData.Spawn((point, dir) =>
                {
                    var pos = (Vector3) point;

                    var extraRotation = Quaternion.LookRotation(Vector3.forward, obj.up);
                    
                    var fireRate = idx == 0 ? Main.FireRate : 0;
                    if (!burstData.burstsUpdatePositionEveryBullet)
                        fireRate = burstNum == 0 ? fireRate : 0;
                    
                    foreach (var module in spawnModules)
                        module?.Execute(ref pos, ref extraRotation, fireRate);
                    
                    foreach (var module in allModules)
                        if (module is IBulletSpawnModule mod)
                            mod.Execute(ref pos, ref extraRotation, fireRate);
                    // rotate dir by this new rotation
                    Vector3 rotatedDir = extraRotation * dir;
                    Quaternion rotation = Quaternion.LookRotation(Vector3.forward, rotatedDir);
                    
                    
                    Vector3 spawnPosition = obj.position + extraRotation * pos;

                    rotations.Add(rotation);
                    positions.Add(spawnPosition);
                    ++idx;
                }, Squirrel3.Instance);
                
                // for every bullet we found
                for (int i = 0; i < positions.Count; i++)
                {
                    // create a new container that isn't dead, at the position and rotation we found with the spawner
                    var newContainer = new BulletContainer
                    {
                        Dead = 0,
                        Position = positions[i],
                        Rotation = rotations[i],
                        Direction = rotations[i]
                    }; 
                    
                    var j = 0;
                    // find a bullet that isn't alive and replace it with this one
                    for (j = 0; j < _bullets.Length; ++j)
                    {
                        if (_bullets[j].Dead == 0) continue;
                        newContainer.Id = j;
                        _bullets[j] = newContainer;
                        break;
                    }
                    if ((burstData.maxActiveBullets == 0 && j >= _bullets.Length) || (burstData.maxActiveBullets > 0 && j >= burstData.maxActiveBullets))
                    {
                        #if UNITY_EDITOR
                        Debug.LogWarning($"Tried to spawn too many bullets on manager {name}, didn't spawn one.");
                        #endif
                        return;
                    }
                    // initialise the bullet
                    var bullet = _bullets[j];
                    
                    bullet.Damage = main.Damage;
                    bullet.Lifetime = main.Lifetime;
                    bullet.Speed = main.Speed;
                    bullet.CurrentSpeed = bullet.Speed;
                    bullet.AngularVelocity = 0f;
                    bullet.StartSize = main.StartSize.GetValue(_rnd);
                    bullet.CurrentSize = bullet.StartSize;
                    bullet.StartColor = main.StartColor;
                    bullet.Color = bullet.StartColor;
                    bullet.ColliderSize = bullet.CurrentSize * main.ColliderSize / 2f;
                    bullet.UseCapsule = main.ColliderType == ColliderType.Capsule ? (byte) 1 : (byte) 0;
                    bullet.CapsuleLength = main.CapsuleLength;
                    bullet.MovingToOrigin = 0;

                    foreach (var module in allModules)
                    {
                        if (module is IBulletInitModule initMod)
                            initMod.Execute(ref bullet);
                        if (module is IBulletModule bulletMod)
                            bulletMod.Execute(ref bullet, deltaTime);
                    }

                    bullet.Speed += burstNum * burstData.stackSpeedIncrease;
                    bullet.CurrentSpeed += burstNum * burstData.stackSpeedIncrease;
                    _bullets[j] = bullet;
                    
                    ++_bulletCount;
                    OnBulletSpawned?.Invoke(j, _bullets[j]);
                    OnBulletSpawnedEvent?.Invoke(j, _bullets[j]);
                }
                
                // wait a little bit before doing the next burst
                #if UNITY_2023_1_OR_NEWER
                // wait a little bit before doing the next burst
                await Awaitable.WaitForSecondsAsync(burstData.burstDelay);
                #else
                var timer = burstData.burstDelay;
                while (timer >= 0)
                {
                    timer -= Time.deltaTime;
                    await Task.Yield();
                }
                #endif
            }
        }
        
        /// <summary>
        /// Activate any waiting bullets.
        /// Use this when you want to do bullet tracing.
        /// </summary>
        public async void ActivateWaitingBullets()
        {
            #if UNITY_2023_1_OR_NEWER
            await Awaitable.EndOfFrameAsync();
            #else
            await Task.Yield();
            #endif
            for (int i = 0; i < _bullets.Length; i++)
            {
                var bullet = _bullets[i];
                bullet.Waiting = 0;
                _bullets[i] = bullet;
            }
            
        }
    }
}