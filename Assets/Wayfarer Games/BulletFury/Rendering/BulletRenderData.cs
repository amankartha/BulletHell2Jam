using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BulletFury
{
    [Serializable]
    public class BulletRenderData
    {
        private static Material _unlitMaterial;
        private static Material _animatedMaterial;
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int Cols = Shader.PropertyToID("_Cols");
        private static readonly int Rows1 = Shader.PropertyToID("_Rows");
        private static readonly int Frame = Shader.PropertyToID("_Frame");

        public Camera Camera;
        public Texture2D Texture;
        public bool Animated;
        [Min(1)]
        public int Rows = 1, Columns = 1;
        public float PerFrameLength;
        public int Layer;
        public int Priority;
        
        private Material _material = null;

        public Material Material
        {
            get
            {
                if (_unlitMaterial == null || _animatedMaterial == null)
                {
                    _material = null;
                    
#if UNITY_WEBGL
                    _unlitMaterial = new Material(Shader.Find("Shader Graphs/UnlitBulletWeb"))
#else
                    _unlitMaterial = new Material(Shader.Find("Shader Graphs/UnlitBullet"))
#endif
                    {
                        enableInstancing = true
                    };
#if UNITY_WEBGL
                    _animatedMaterial = new Material(Shader.Find("Shader Graphs/AnimatedBulletWeb"))
#else
                    _animatedMaterial = new Material(Shader.Find("Shader Graphs/AnimatedBullet"))
#endif
                    {
                        enableInstancing = true
                    };
                }
                
                if (_material == null)
                {
                    _material = !Animated ? Object.Instantiate(_unlitMaterial) : Object.Instantiate(_animatedMaterial);
                    _material.SetTexture(MainTex, Texture);
                    if (Animated)
                    {
                        _material.SetInt(Cols, Columns);
                        _material.SetInt(Rows1, Rows);
                        _material.SetFloat(Frame, PerFrameLength);
                    }
                }
                return _material;
            }
        }

        public static void ResetMaterials()
        {
            _unlitMaterial = null;
            _animatedMaterial = null;
        }
    }
}