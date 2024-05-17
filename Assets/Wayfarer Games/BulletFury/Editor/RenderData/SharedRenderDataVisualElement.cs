using System;
using System.Linq;
using BulletFury;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Wayfarer_Games.BulletFury.RenderData
{
    public class SharedRenderDataVisualElement : BindableElement
    {
        public new class UxmlFactory : UxmlFactory<SharedRenderDataVisualElement, UxmlTraits>
        {
        }

        private static VisualTreeAsset UXML;
        
        public event Action<ChangeEvent<Object>> SharedDataChanged; 
        private SerializedProperty _singleData, _sharedData;


        public RenderDataVisualElement SingleData { get; private set; }
        public RenderDataVisualElement SharedData { get; private set; }

        public (Image, Image) GetPreview()
        {
            return (SingleData.Preview, SharedData.Preview);
        }
        
        public (VisualElement[], VisualElement[]) GetColliderPreview()
        {
            return (SingleData.ColliderPreview, SharedData.ColliderPreview);
        }

        public SharedRenderDataVisualElement()
        {
            if (UXML == null)
                UXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/Wayfarer Games/BulletFury/Editor/RenderData/RenderData.uxml");

            UXML.CloneTree(this);
            
            SingleData = this.Q<RenderDataVisualElement>("SingleData");
            SharedData = this.Q<RenderDataVisualElement>("SharedData");
            
            this.Q<ObjectField>("SharedDataSO").objectType = typeof(SharedRenderDataSO);
        }
        
        public void InitWithProperties (SerializedProperty singleData, [CanBeNull] SerializedProperty sharedData)
        {
            _singleData = singleData;
            _sharedData = sharedData;
            
            SingleData.InitWithProperty(singleData);
            if (sharedData != null)
                SharedData.InitWithProperty(sharedData);

            if (sharedData != null)
            {
                #if UNITY_2022_1_OR_NEWER
                SingleData.style.display =
                    sharedData.boxedValue == null ? DisplayStyle.Flex : DisplayStyle.None;
                SharedData.style.display =
                    sharedData.boxedValue == null ? DisplayStyle.None : DisplayStyle.Flex;
                #else
                SingleData.style.display =
                    sharedData.objectReferenceValue == null ? DisplayStyle.Flex : DisplayStyle.None;
                SharedData.style.display =
                    sharedData.objectReferenceValue == null ? DisplayStyle.None : DisplayStyle.Flex;
                #endif
            }
            else
            {
                SingleData.style.display = DisplayStyle.Flex;
                SharedData.style.display = DisplayStyle.None;
            }

            this.Q<ObjectField>("SharedDataSO").RegisterCallback<ChangeEvent<Object>>(OnSharedDataChanged);

            this.Q<Button>("Create").clicked -= OnClicked;
            this.Q<Button>("Create").clicked += OnClicked;
            return;

            void OnClicked()
            {
                var path = EditorUtility.SaveFilePanelInProject("Save Render Data", "New Render Data", "asset", "Save Render Data");
                var sharedDataSO = ScriptableObject.CreateInstance<SharedRenderDataSO>();
                var currentData = new BulletRenderData
                {
                    Texture = singleData.FindPropertyRelative("Texture").objectReferenceValue as Texture2D,
                    Animated = singleData.FindPropertyRelative("Animated").boolValue,
                    Rows = singleData.FindPropertyRelative("Rows").intValue,
                    Columns = singleData.FindPropertyRelative("Columns").intValue,
                    PerFrameLength = singleData.FindPropertyRelative("PerFrameLength").floatValue,
                    Layer = singleData.FindPropertyRelative("Layer").intValue,
                    Priority = singleData.FindPropertyRelative("Priority").intValue
                };
                sharedDataSO.SetData(currentData);
                AssetDatabase.CreateAsset(sharedDataSO, path);
                if (sharedData != null)
                    sharedData.objectReferenceValue = sharedDataSO;
            }
        }

        private void OnSharedDataChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue == evt.previousValue) return;
            
            SharedDataChanged?.Invoke(evt);
            if (evt.newValue == null)
            {
                InitWithProperties(_singleData, null);
            }
            else
            {
                var sharedData = new SerializedObject(evt.newValue).FindProperty("data");
                InitWithProperties(_singleData, sharedData);
            }
        }
    }
}