using System;
using System.Collections.Generic;
using System.Linq;
using BulletFury;
using BulletFury.Data;
using BulletFury.Modules;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.Rendering;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Wayfarer_Games.BulletFury.RenderData;
using Object = UnityEngine.Object;
using PopupWindow = UnityEditor.PopupWindow;

namespace Wayfarer_Games.BulletFury
{
    [CustomEditor(typeof(BulletSpawner))]
    public class BulletSpawnerEditor : Editor
    {
        public VisualTreeAsset UXML;
        private VisualElement _root;
        private Image _preview, _sharedPreview;
        private VisualElement _animatedProperties;
        private VisualElement[] _colliderPreview, _sharedColliderPreview;
        private VisualElement _visualBody;
        private VisualElement _shapeBody;
        private VisualElement _group;
        private PropertyField _colliderSpacing;
        private HelpBox _shapeHelp;
        private Label _frameCount;

        private int _currentFrame;

        private BulletSpawner _spawner;

        private Vector3 _previousPos;
        private Vector3 _previousRot;
        private Camera _sceneCamera;
        private SharedRenderDataSO _data;

        private const float DeltaTime = 1 / 60f;
        private double _previousTime;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.RegisterCallback<ChangeEvent<int>>(AnyObjectHasChanged);
            root.RegisterCallback<ChangeEvent<float>>(AnyObjectHasChanged);
            root.RegisterCallback<ChangeEvent<bool>>(AnyObjectHasChanged);
            UXML.CloneTree(root);
            _root = root;
            BuildRenderData(ref root);
            BuildVisualData(ref root);
            BuildSpawnShape(ref root);
            return root;
        }

        private void AnyObjectHasChanged(ChangeEvent<int> evt)
        {
            BulletFuryEditorUtils.RepaintScene();
        }

        private void AnyObjectHasChanged(ChangeEvent<float> evt)
        {
            BulletFuryEditorUtils.RepaintScene();
        }

        private void AnyObjectHasChanged(ChangeEvent<bool> evt)
        {
            BulletFuryEditorUtils.RepaintScene();
        }

        private void OnEnable()
        {
#if !SERIALIZEREFERENCE_EXTENSIONS
            BulletFuryEditorUtils.AddPackage();
#endif
            if (Application.isPlaying) return;
            _previousTime = EditorApplication.timeSinceStartup;
            _spawner = target as BulletSpawner;
            _spawner.Start();

            SceneView.duringSceneGui += SceneGUI;
            BulletRenderer.Init();
        }

        private void OnDisable()
        {
            if (Application.isPlaying) return;
            _spawner = target as BulletSpawner;
            _spawner.OnDestroy();
            SceneView.duringSceneGui -= SceneGUI;
            BulletRenderer.Dispose();
            BulletRenderData.ResetMaterials();
        }

        private void SceneGUI(SceneView obj)
        {
            if (Application.isPlaying) return;
            _spawner.UpdateAllBullets(obj.camera, Convert.ToSingle(EditorApplication.timeSinceStartup - _previousTime));
            _spawner.LateUpdate();
            _previousTime = EditorApplication.timeSinceStartup;
            obj.Repaint();
        }
        //
        // private void BuildModules(ref VisualElement root)
        // {
        //     var spawnModules = serializedObject.FindProperty("spawnModules");
        //     var spawnModulesRoot = root.Q<ListView>("SpawnModules");
        //     spawnModulesRoot.itemsRemoved += SpawnModulesRootOnItemsRemoved;
        // }
        //
        // private void SpawnModulesRootOnItemsRemoved(IEnumerable<int> obj)
        // {
        //     var spawnModules = serializedObject.FindProperty("spawnModules");
        //     var spawnModulesRoot = _root.Q<ListView>("SpawnModules");
        //
        //     spawnModules.serializedObject.Update();
        //     spawnModules.serializedObject.ApplyModifiedProperties();
        //     spawnModulesRoot.Rebuild();
        // }

        private void BuildSpawnShape(ref VisualElement root)
        {
            root.Q<VisualElement>("ShapeHeader").RegisterCallback<ClickEvent>(ToggleShape);
            root.Q<PropertyField>("SpawnDir").RegisterValueChangeCallback(ChangeSpawnDir);
            _shapeHelp = root.Q<HelpBox>("SpawnDirHelp");
            _shapeBody = root.Q<VisualElement>("ShapeBody");
            _group = root.Q<VisualElement>("Group");

            root.Q<PropertyField>("NumPerSide").RegisterCallback<ChangeEvent<int>>(ChangeNumPerSide);

            var prop = serializedObject.FindProperty("spawnShapeData").FindPropertyRelative("spawnDir");
            if (Enum.TryParse<SpawnDir>(prop.enumNames[prop.enumValueIndex], out var dir))
            {
                _shapeHelp.text = dir switch
                {
                    SpawnDir.Shape =>
                        "Bullets will travel in the direction of the shape's edge, e.g. for a square, they will travel up, down, left, or right.",
                    SpawnDir.Randomised => "Bullets will travel in a random direction",
                    SpawnDir.Spherised =>
                        "Bullets will travel away from the center of the shape, which will create a circular pattern",
                    SpawnDir.Direction =>
                        $"Bullets will travel in the direction of the spawner's UP vector - the green arrow in the scene view.",
                    SpawnDir.Point =>
                        "Bullets will travel in the direction of shape's vertex, e.g. for a square, they will travel in the diagonal directions.",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        private void ChangeNumPerSide(ChangeEvent<int> evt)
        {
            if (evt.newValue == 1)
                _group.RemoveFromClassList("collapsed");
            else
                _group.AddToClassList("collapsed");
        }

        private void ChangeSpawnDir(SerializedPropertyChangeEvent evt)
        {
            if (Enum.TryParse<SpawnDir>(evt.changedProperty.enumNames[evt.changedProperty.enumValueIndex], out var dir))
            {
                _shapeHelp.text = dir switch
                {
                    SpawnDir.Shape =>
                        "Bullets will travel in the direction of the shape's edge, e.g. for a square, they will travel up, down, left, or right.",
                    SpawnDir.Randomised => "Bullets will travel in a random direction",
                    SpawnDir.Spherised =>
                        "Bullets will travel away from the center of the shape, which will create a circular pattern",
                    SpawnDir.Direction =>
                        $"Bullets will travel in the direction of the spawner's UP vector - the green arrow in the scene view.",
                    SpawnDir.Point =>
                        "Bullets will travel in the direction of shape's vertex, e.g. for a square, they will travel in the diagonal directions.",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        private void BuildVisualData(ref VisualElement root)
        {
            root.Q<PropertyField>("StartColor").RegisterValueChangeCallback(ChangeColor);
            root.Q<PropertyField>("ColliderSize").RegisterCallback<ChangeEvent<float>>(ChangeColliderSize);
            //root.Q<VisualElement>("VisualHeader").RegisterCallback<ClickEvent>(ToggleVisual);
            //_visualBody = root.Q<VisualElement>("VisualBody");

            root.Q<PropertyField>("ColliderType").RegisterCallback<ChangeEvent<string>>(ChangeColliderCount);
            _colliderSpacing = root.Q<PropertyField>("ColliderSeparation");
            _colliderSpacing.RegisterCallback<ChangeEvent<float>>(ChangeColliderSeparation);
        }

        private void GrabPreviewAndChildren(VisualElement root)
        {
            if (_root == null) return;
            (_preview, _sharedPreview) = root.Q<SharedRenderDataVisualElement>().GetPreview();
            (_colliderPreview, _sharedColliderPreview) = root.Q<SharedRenderDataVisualElement>().GetColliderPreview();
        }

        private void ChangeColliderSeparation(ChangeEvent<float> evt)
        {
            GrabPreviewAndChildren(_root);
            if (_preview == null) return;

            var useCollider =
                serializedObject.FindProperty("main").FindPropertyRelative("ColliderType").enumValueIndex ==
                (int)ColliderType.Capsule;
            if (useCollider)
            {
                for (int i = 0; i < _colliderPreview.Length; ++i)
                {
                    var pos = _colliderPreview[i].transform.position;
                    pos.y = evt.newValue * (i - 0.5f) * 100;
                    _colliderPreview[i].transform.position = pos;
                }

                for (int i = 0; i < _sharedColliderPreview.Length; ++i)
                {
                    var pos = _sharedColliderPreview[i].transform.position;
                    pos.y = evt.newValue * (i - 0.5f) * 100;
                    _sharedColliderPreview[i].transform.position = pos;
                }
            }
        }

        private void ChangeColliderCount(ChangeEvent<string> evt)
        {
            GrabPreviewAndChildren(_root);
            if (_preview == null) return;

            if (evt.newValue == ColliderType.Capsule.ToString())
                _colliderSpacing.RemoveFromClassList("collapsed");
            else
                _colliderSpacing.AddToClassList("collapsed");

            var element = _colliderPreview[1];
            if (evt.newValue == ColliderType.Capsule.ToString())
                element.RemoveFromClassList("collapsed");
            else
                element.AddToClassList("collapsed");

            element = _sharedColliderPreview[1];
            if (evt.newValue == ColliderType.Capsule.ToString())
                element.RemoveFromClassList("collapsed");
            else
                element.AddToClassList("collapsed");

            var spacing = serializedObject.FindProperty("main").FindPropertyRelative("CapsuleLength").floatValue;

            if (evt.newValue == ColliderType.Capsule.ToString())
            {
                for (int i = 0; i < _colliderPreview.Length; ++i)
                {
                    var pos = _colliderPreview[i].transform.position;
                    pos.y = spacing * (i - 0.5f) * 100;
                    _colliderPreview[i].transform.position = pos;
                }

                for (int i = 0; i < _sharedColliderPreview.Length; ++i)
                {
                    var pos = _sharedColliderPreview[i].transform.position;
                    pos.y = spacing * (i - 0.5f) * 100;
                    _sharedColliderPreview[i].transform.position = pos;
                }
            }
            else
            {
                var pos = _colliderPreview[0].transform.position;
                pos.y = 0;
                _colliderPreview[0].transform.position = pos;

                pos = _sharedColliderPreview[0].transform.position;
                pos.y = 0;
                _sharedColliderPreview[0].transform.position = pos;
            }
        }

        private void ToggleShape(ClickEvent evt)
        {
            _shapeBody.ToggleInClassList("collapsed");
        }

        private void ToggleVisual(ClickEvent evt)
        {
            _visualBody.ToggleInClassList("collapsed");
        }

        private void ChangeColliderSize(ChangeEvent<float> evt)
        {
            GrabPreviewAndChildren(_root);
            if (_preview == null) return;

            foreach (var p in _colliderPreview)
                p.transform.scale = Vector3.one * evt.newValue;

            foreach (var p in _sharedColliderPreview)
                p.transform.scale = Vector3.one * evt.newValue;
        }

        private void ChangeColor(SerializedPropertyChangeEvent evt)
        {
            GrabPreviewAndChildren(_root);
            if (_preview == null) return;

            if (evt?.changedProperty == null) return;
            var color = evt.changedProperty.colorValue;
            _preview.tintColor = color;
            _sharedPreview.tintColor = color;
        }

        private void BuildRenderData(ref VisualElement root)
        {
            root.Q<PropertyField>("Script").SetEnabled(false);
            var renderData = serializedObject.FindProperty("renderData");
            var singleData = renderData.FindPropertyRelative("singleData");

            _data = (target as BulletSpawner).RenderData.SharedDataSO;


            var sharedRenderDataVisualElement = root.Q<SharedRenderDataVisualElement>();

            if (_data != null)
            {
                var targetObj = new SerializedObject(_data);
                sharedRenderDataVisualElement.InitWithProperties(singleData, targetObj.FindProperty("data"));
            }
            else
                sharedRenderDataVisualElement.InitWithProperties(singleData, null);

            sharedRenderDataVisualElement.SharedDataChanged += SharedDataChanged;

        }

        private void SharedDataChanged(ChangeEvent<Object> obj)
        {
            if (obj.newValue == null)
            {
                _data = null;
                return;
            }

            _data = obj.newValue as SharedRenderDataSO;
        }
    }
}