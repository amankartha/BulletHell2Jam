using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BulletFury.Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace BulletFury
{
    public static class BulletRenderer
    {
        private static readonly int SortOrder = Shader.PropertyToID("_SortOrder");
        private const int BulletsPerChunk = 1023;
        private static HashSet<(BulletRenderData, NativeArray<BulletContainer>)> _toRender;
        private static Mesh _mesh;
        public static Mesh Mesh => _mesh;
        private static NativeArray<BulletContainer> _chunk = new(BulletsPerChunk, Allocator.Persistent);
        
        private static NativeArray<Matrix4x4> _matrices;
        private static NativeArray<Vector4> _colors;
        private static NativeArray<float> _tex;
        private static Dictionary<BulletRenderData, (GraphicsBuffer color, GraphicsBuffer tex)> _colorBuffer;

        private static bool _hasBullets;
        private static readonly int InstanceColorBuffer = Shader.PropertyToID("_InstanceColorBuffer");
        private static readonly int InstanceTexBuffer = Shader.PropertyToID("_InstanceTexBuffer");

        private static int _newLength;
        private static int _num;
        private static bool _alreadyInitialised;

        private static bool _disposed = false;


        #if UNITY_EDITOR
        static BulletRenderer()
        {
            Init();
        }
        #endif
        
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            InitMesh();
            _toRender = new HashSet<(BulletRenderData, NativeArray<BulletContainer>)>();
            _matrices = new NativeArray<Matrix4x4>(BulletsPerChunk, Allocator.Persistent);
            _colors = new NativeArray<Vector4>(BulletsPerChunk, Allocator.Persistent);
            _tex = new NativeArray<float>(BulletsPerChunk, Allocator.Persistent);
            _chunk = new NativeArray<BulletContainer>(BulletsPerChunk, Allocator.Persistent);
            _colorBuffer = new Dictionary<BulletRenderData, (GraphicsBuffer color, GraphicsBuffer tex)>();

            _disposed = false;
            
            LateUpdate();
            if (_alreadyInitialised) return;
            _alreadyInitialised = true;
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += PlayModeStateChanged;
            #endif
        }

        #if UNITY_EDITOR
        private static void PlayModeStateChanged(UnityEditor.PlayModeStateChange obj)
        {
            if (obj == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                Dispose();
            if (obj == UnityEditor.PlayModeStateChange.EnteredEditMode)
                Init();
        }
        #endif
        private static async void LateUpdate()
        {
            while (!_disposed)
            {
                if (_disposed)
                {
                    BulletSpawner.RenderQueue.Clear();
                    return;
                }
                foreach (var queue in BulletSpawner.RenderQueue.Values)
                    if (!queue.Spawner.Disposed)
                        Render(queue.RenderData, queue.Bullets, queue.Count, queue.RenderData.Camera);
                
                BulletSpawner.RenderQueue.Clear();
                
                await Task.Yield();
                
                if (_disposed)
                {
                    BulletSpawner.RenderQueue.Clear();
                    return;
                }
            }
        }
        
        public static void Dispose()
        {
            Application.quitting -= Dispose;
            _matrices.Dispose();
            _colors.Dispose();
            foreach (var buf in _colorBuffer.Values)
            {
                buf.color.Dispose();
                buf.tex.Dispose();
            }
            _chunk.Dispose();
            _disposed = true;
            BulletSpawner.RenderQueue.Clear();
        }

        public static void Render(BulletRenderData data, NativeArray<BulletContainer> bullets, int numBullets, Camera cam)
        {
            if (numBullets == 0 || _disposed) return;

            data.Material.SetFloat(SortOrder, data.Priority);

            _num = 0;
            while (_num < numBullets)
            {
                _newLength = Mathf.Min(bullets.Length - _num, BulletsPerChunk);
                NativeArray<BulletContainer>.Copy(bullets, _num, _chunk, 0, _newLength);
                RenderChunk(data, _chunk, _newLength, cam);
                _num += _newLength;
            }
        }

        private static void RenderChunk(BulletRenderData data, NativeArray<BulletContainer> bullets, int length, Camera cam)
        {
            #if !UNITY_WEBGL
            if (!_colorBuffer.ContainsKey(data))
                _colorBuffer.Add(data,  (new GraphicsBuffer (GraphicsBuffer.Target.Structured,
                    BulletsPerChunk, sizeof(float) * 4), new GraphicsBuffer (GraphicsBuffer.Target.Structured,
                    BulletsPerChunk, sizeof(float))));
            #endif
            // create a new material property block - this contains the different colours for every instance
            var renderParams = new RenderParams(data.Material)
            {
                layer = data.Layer,
                camera = cam,
                rendererPriority = data.Priority,
            };
            
            _hasBullets = false;

            for (int i = bullets.Length - 1; i >= 0; --i)
            {
                if (bullets[i].Dead == 1 || i >= length)
                {
                    _matrices[i] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.zero);
                    continue;
                }
                
                _hasBullets = true;
                
                // set the colour for the bullet
                _colors[i] = bullets[i].Color;
                _tex[i] = bullets[i].CurrentLifeSeconds;
                    
                // if the "w" part of the rotation is 0, the Quaternion is invalid. Set it to the a rotation of 0,0,0
                if (Mathf.Approximately(bullets[i].Rotation.w, 0) || IsNaN(bullets[i].Rotation))
                    _matrices[i] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.zero);
                else
                {
                    // set the matrix for the current bullet - translation, rotation, scale, in that order.
                    _matrices [i] = Matrix4x4.TRS(bullets[i].Position,
                        bullets[i].Rotation,
                        Vector3.one * bullets[i].CurrentSize);
                }
                
                #if UNITY_WEBGL
                var mat = Object.Instantiate(data.Material);
                mat.SetColor("_Color", bullets[i].Color);
                renderParams.material = mat;
                Graphics.RenderMesh(renderParams, _mesh, 0, _matrices[i]);
                #endif
            }

            #if !UNITY_WEBGL
            
            if (!_hasBullets)
                return;
            
            _colorBuffer[data].color.SetData(_colors);
            _colorBuffer[data].tex.SetData(_tex);
            data.Material.SetBuffer(InstanceColorBuffer, _colorBuffer[data].color);
            data.Material.SetBuffer(InstanceTexBuffer, _colorBuffer[data].tex);
            
            Graphics.RenderMeshInstanced(renderParams, _mesh, 0, _matrices, length);
            #endif
        }
        
        private static bool IsNaN(Quaternion q) {
            return float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);
        }

        private static void InitMesh()
        {
            var vertices = new Vector3[]
            {
                new(-0.5f, -0.5f),
                new(0.5f, -0.5f),
                new(-0.5f, 0.5f),
                new(0.5f, 0.5f)
            };

            var triangles = new[]
            {
                0, 3, 1,
                3, 0, 2
            };

            var uv = new Vector2[]
            {
                new(0, 0),
                new(1, 0),
                new(0, 1),
                new(1, 1)
            };

            _mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uv
            };
        }
    }
}