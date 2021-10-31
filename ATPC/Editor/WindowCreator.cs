using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Object = System.Object;

namespace alisahanyalcin
{

    public class WindowCreator : EditorWindow
    {
        private VisualElement _container;
        private ObjectField _player;
        private ObjectField _animator;
        private ObjectField _avatar;
        private ObjectField _camera;
        private TagField _playerTag;
        private LayerField _playerLayer;
        private LayerMaskField _walkableLayer;
        private HelpBox _helpBox;
        private Button _createButton;

        private const string Path = "Assets/ATPC/";

        [MenuItem("ATPC/Editor")]
        private static void ShowWindow()
        {
            WindowCreator windowCreator = GetWindow<WindowCreator>("ATPC Editor");
            windowCreator.minSize = new Vector2(430, 500);
        }

        public void CreateGUI()
        {
            _container = rootVisualElement;
            VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{Path}UI Toolkit/creator.uxml");
            original.CloneTree(_container);

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{Path}UI Toolkit/creator.uss");
            _container.styleSheets.Add(styleSheet);

            _player = _container.Q<ObjectField>("player");

            _animator = _container.Q<ObjectField>("animator");
            _camera = _container.Q<ObjectField>("camera");

            _avatar = _container.Q<ObjectField>("avatar");
            _avatar.objectType = typeof(Avatar);

            _playerTag = _container.Q<TagField>("playerTag");

            _playerLayer = _container.Q<LayerField>("playerLayer");

            _walkableLayer = _container.Q<LayerMaskField>("walkableLayer");

            _createButton = _container.Q<Button>("createButton");

            _helpBox = _container.Q<HelpBox>("helpBox");
            _helpBox.visible = false;

            _createButton.clicked += CreatePlayer;
        }

        private void CreatePlayer()
        {
            if (_player.value != null && _animator.value != null && _avatar.value != null && _camera.value != null)
            {
                GameObject playerObject = (GameObject)Instantiate(_player.value, Vector3.zero, Quaternion.identity);
                playerObject.name = "Person";
                playerObject.layer = _playerLayer.value;
                playerObject.tag = _playerTag.value;

                new GameObject("spherePosition").AddComponent<EditorGizmo>().transform.SetParent(playerObject.transform);
                var eGizmo = GameObject.Find("spherePosition").GetComponent<EditorGizmo>();
                eGizmo.shapes = EditorGizmo.GizmoShape.Sphere;
                eGizmo.gizmoSize = 0.2f;
                eGizmo.gizColor = new Color(0, 255, 201, 255);

                new GameObject("cameraPivot").transform.SetParent(playerObject.transform);
                var cameraPivot = GameObject.Find("cameraPivot");
                cameraPivot.transform.position = new Vector3(0, 1.375f, 0);

                var anim = playerObject.AddComponent<Animator>();
                anim.runtimeAnimatorController = (RuntimeAnimatorController)_animator.value;
                anim.avatar = (Avatar)_avatar.value;

                var cc = playerObject.AddComponent<CharacterController>();
                cc.stepOffset = 0.25f;
                cc.skinWidth = 0.02f;
                cc.minMoveDistance = 0f;
                cc.center = new Vector3(0, 0.93f, 0);
                cc.radius = 0.28f;
                cc.height = 1.8f;

                var tpc = playerObject.AddComponent<ThirdPersonController>();
                tpc.animator = anim;
                tpc.controller = cc;
                tpc.groundLayers = _walkableLayer.value;
                tpc.input = tpc.AddComponent<ATPCInputs>();
                tpc.spherePosition = eGizmo.transform.GameObject();
                tpc.cinemachineCameraTarget = cameraPivot.GameObject();

                var playerInput = tpc.AddComponent<PlayerInput>();
                playerInput.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>($"{Path}InputSystem/ATPC.inputactions");
                playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;

                GameObject cameraObject = (GameObject)Instantiate(_camera.value, Vector3.zero, Quaternion.identity);
                cameraObject.name = "Camera";
                cameraObject.GetComponentInChildren<CinemachineFreeLook>().Follow = playerObject.transform;
                cameraObject.GetComponentInChildren<CinemachineFreeLook>().LookAt = cameraPivot.transform;
                for (var i = 0; i <= 2; i++)
                    cameraObject.GetComponentInChildren<CinemachineFreeLook>().GetRig(i).m_LookAt = cameraPivot.transform;

                tpc.mainCamera = cameraObject.GetComponentInChildren<Camera>();

                CreateInfo(_helpBox, HelpBoxMessageType.Info, "Created.");
            }
            else
                CreateInfo(_helpBox, HelpBoxMessageType.Error, "Make sure ObjectFields are not empty.");
        }

        private static void CreateInfo(HelpBox box, HelpBoxMessageType type, string message)
        {
            box.visible = true;
            box.messageType = type;
            box.text = message;
        }
    }
}