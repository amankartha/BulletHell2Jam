using System.Linq;
using BulletFury;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Wayfarer_Games.BulletFury.RenderData
{
    public class RenderDataVisualElement : BindableElement
    {
        public new class UxmlFactory : UxmlFactory<RenderDataVisualElement, UxmlTraits>
        {
        }

        private static VisualTreeAsset UXML;

        public Image Preview { get; private set; }

        public VisualElement[] ColliderPreview { get; private set; }
        
        
        private SerializedProperty _property;
        private Image _preview;
        private VisualElement _animatedProperties;
        private VisualElement[] _colliderPreview;
        private Label _frameCount;
        private int _currentFrame;

        public RenderDataVisualElement()
        {
            if (UXML == null)
                UXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/Wayfarer Games/BulletFury/Editor/RenderData/RenderDataTextureSettings.uxml");

            UXML.CloneTree(this);

            Preview = this.Q<Image>("Preview");

            ColliderPreview = Preview.Children().ToArray();
        }

        public void InitWithProperty(SerializedProperty property)
        {
            if (property == null) return;
            
            _property = property;

            this.BindProperty(property);
            
            
            this.Q<ObjectField>("Texture").RegisterCallback<ChangeEvent<Object>>(SetTexture);
            _animatedProperties = this.Q<VisualElement>("AnimatedProperties");
            if (property.FindPropertyRelative("Animated").boolValue)
                _animatedProperties.RemoveFromClassList("hidden");
            else 
                _animatedProperties.AddToClassList("hidden");
            
            this.Q<PropertyField>("Animated").RegisterCallback<ChangeEvent<bool>>(ChangeAnimated);
            this.Q<PropertyField>("Rows").RegisterCallback<ChangeEvent<int>>(ChangeRows);
            this.Q<PropertyField>("Columns").RegisterCallback<ChangeEvent<int>>(ChangeColumns);

            _frameCount = this.Q<Label>("FrameCount");
            
            _currentFrame = 0;
            var uv = Preview.uv;
            uv.x = 0;
            uv.y = 0;
            Preview.uv = uv;
            Preview.MarkDirtyRepaint();
            _frameCount.text = "0 " +
                               property.FindPropertyRelative("Rows").intValue *
                               property.FindPropertyRelative("Columns").intValue;
            
            this.Q<Button>("NextFrame").clicked += () =>
            {
                _currentFrame++;
                if (_currentFrame >= property.FindPropertyRelative("Rows").intValue *
                    property.FindPropertyRelative("Columns").intValue)
                    _currentFrame = 0;
                
                var uv = Preview.uv;
                uv.x += uv.width;
                if (uv.x >= 1)
                {
                    uv.x = 0;
                    uv.y -= uv.height;
                    
                    if (uv.y < 0)
                        uv.y = 1-uv.height;
                }
                Preview.uv = uv;
                Preview.MarkDirtyRepaint();
                _frameCount.text = $"Frame {_currentFrame} / " +
                                   property.FindPropertyRelative("Rows").intValue *
                                   property.FindPropertyRelative("Columns").intValue;
            };

            this.Q<Button>("PreviousFrame").clicked += () =>
            {
                _currentFrame--;
                if (_currentFrame < 0)
                    _currentFrame = property.FindPropertyRelative("Rows").intValue *
                                    property.FindPropertyRelative("Columns")
                                        .intValue;
                
                var uv = Preview.uv;
                uv.x -= uv.width;
                if (uv.x < 0)
                {
                    uv.x = 1-uv.width;
                    uv.y += uv.height;
                    
                    if (uv.y > 1)
                        uv.y = 0;
                }
                Preview.uv = uv;
                Preview.MarkDirtyRepaint();
                _frameCount.text = $"Frame {_currentFrame} / " +
                                   property.FindPropertyRelative("Rows").intValue *
                                   property.FindPropertyRelative("Columns").intValue;
            };

        }
        
        private void ChangeColumns(ChangeEvent<int> evt)
        {
            var uv = Preview.uv;
            uv.height = 1f / evt.newValue;
            Preview.uv = uv;
            Preview.MarkDirtyRepaint();
            _frameCount.text = $"Frame {_currentFrame} / " +
                               _property.FindPropertyRelative("Rows").intValue *
                               _property.FindPropertyRelative("Columns").intValue;
        }

        private void ChangeRows(ChangeEvent<int> evt)
        {
            var uv = Preview.uv;
            uv.width = 1f / evt.newValue;
            Preview.uv = uv;
            Preview.MarkDirtyRepaint();
            _frameCount.text = $"Frame {_currentFrame} / " +
                               _property.FindPropertyRelative("Rows").intValue *
                               _property.FindPropertyRelative("Columns").intValue;
        }

        private void ChangeAnimated(ChangeEvent<bool> evt)
        {
            _animatedProperties.SetEnabled(evt.newValue);
            if (evt.newValue)
            {
                var uv = Preview.uv;
                uv.width = 1f / _property.FindPropertyRelative("Rows").intValue;
                uv.height = 1f/ _property.FindPropertyRelative("Columns").intValue;
                Preview.uv = uv;
                Preview.MarkDirtyRepaint();
                _animatedProperties.RemoveFromClassList("hidden");
            }
            else
            {
                _animatedProperties.AddToClassList("hidden");
                var uv = Preview.uv;
                uv.width = 1f;
                uv.height = 1f;
                Preview.uv = uv;
                Preview.MarkDirtyRepaint();
            }
            
        }
        
        private void SetTexture(ChangeEvent<Object> evt)
        {
            Preview.image = evt.newValue as Texture2D;
        }

    }
}