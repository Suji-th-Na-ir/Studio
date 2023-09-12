using System;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    public class ToolbarView : View
    {
        private const string ADD_BUTTON_LOC = "AddButton";
        private const string PLAY_BUTTON_LOC = "PlayButton";
        private const string SAVE_BUTTON_LOC = "SaveButton";
        private const string LOAD_BUTTON_LOC = "LoadButton";
        private const string CYLINDER_PRIMITIVE_BUTTON_LOC = "cylinder_button";
        private const string SPHERE_PRIMITIVE_BUTTON_LOC = "sphere_button";
        private const string CUBE_PRIMITIVE_BUTTON_LOC = "cube_button";
        private const string PLANE_PRIMITIVE_BUTTON_LOC = "plane_button";
        private const string CHECKPOINT_PRIMITIVE_BUTTON_LOC = "checkpoint_button";
        private const string TIMER_BUTTON_LOC = "timer_button";

        [SerializeField] private GameObject PrimitivePanel;
        private void Awake()
        {
            EditorOp.Register(this);
        }

        public override void Init()
        {
            PrimitivePanel.SetActive(false);
            var addButtonTr = Helper.FindDeepChild(transform, ADD_BUTTON_LOC, true);
            var playButtonTr = Helper.FindDeepChild(transform, PLAY_BUTTON_LOC, true);
            var saveButtonTr = Helper.FindDeepChild(transform, SAVE_BUTTON_LOC, true);
            var loadButtonTr = Helper.FindDeepChild(transform, LOAD_BUTTON_LOC, true);
            var cylinderPrimitiveTr = Helper.FindDeepChild(transform, CYLINDER_PRIMITIVE_BUTTON_LOC, true);
            var spherePrimitiveTr = Helper.FindDeepChild(transform, SPHERE_PRIMITIVE_BUTTON_LOC, true);
            var cubePrimitiveTr = Helper.FindDeepChild(transform, CUBE_PRIMITIVE_BUTTON_LOC, true);
            var planePrimitiveTr = Helper.FindDeepChild(transform, PLANE_PRIMITIVE_BUTTON_LOC, true);
            var checkpointTr = Helper.FindDeepChild(transform, CHECKPOINT_PRIMITIVE_BUTTON_LOC, true);
            var timerTr = Helper.FindDeepChild(transform, TIMER_BUTTON_LOC, true);

            var addButton = addButtonTr.GetComponent<Button>();
            AddListenerEvent(addButton, () =>
            {
                var currentActiveState = PrimitivePanel.activeSelf;
                PrimitivePanel.SetActive(!currentActiveState);
            });

            var playButton = playButtonTr.GetComponent<Button>();
            AddListenerEvent(playButton, () =>
            {
                EditorOp.Resolve<SceneDataHandler>().PrepareSceneDataToRuntime();
                EditorOp.Resolve<EditorSystem>().RequestSwitchState();
            });

            var saveButton = saveButtonTr.GetComponent<Button>();
            AddListenerEvent(saveButton, EditorOp.Resolve<SceneDataHandler>().Save);

            var cylinderButton = cylinderPrimitiveTr.GetComponent<Button>();
            AddListenerEvent(cylinderButton, CreateObject, PrimitiveType.Cylinder.ToString());

            var sphereButton = spherePrimitiveTr.GetComponent<Button>();
            AddListenerEvent(sphereButton, CreateObject, PrimitiveType.Sphere.ToString());

            var cubeButton = cubePrimitiveTr.GetComponent<Button>();
            AddListenerEvent(cubeButton, CreateObject, PrimitiveType.Cube.ToString());

            var planeButton = planePrimitiveTr.GetComponent<Button>();
            AddListenerEvent(planeButton, CreateObject, PrimitiveType.Plane.ToString());

            var checkpointButton = checkpointTr.GetComponent<Button>();
            AddListenerEvent(checkpointButton, CreateObject, "CheckPoint");

            var timerButton = timerTr.GetComponent<Button>();
            AddListenerEvent(timerButton, CreateObject, "InGameTimer");
        }

        private void AddListenerEvent<T>(Button button, Action<T> callback, T type)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { callback?.Invoke(type); });
        }

        public void CreateObject(string name)
        {
            if (!CanSpawn(name))
            {
                return;
            }
            Transform cameraTransform = Camera.main.transform;
            Vector3 cameraPosition = cameraTransform.position;
            Vector3 spawnPosition = cameraPosition + cameraTransform.forward * 5;
            var itemData = ((ResourceDB)SystemOp.Load(ResourceTag.ResourceDB)).GetItemDataForNearestName(name);
            var primitive = RuntimeWrappers.SpawnGameObject(itemData.ResourcePath, itemData);
            primitive.transform.position = spawnPosition;
            EditorOp.Resolve<SelectionHandler>().OnSelectionChanged(primitive);
            EditorOp.Resolve<SelectionHandler>().SelectObjectInHierarchy(primitive);
            if (name.Equals("CheckPoint"))
            {
                primitive.AddComponent<Checkpoint>();
                EditorOp.Resolve<UILogicDisplayProcessor>().AddComponentIcon(new ComponentDisplayDock { componentGameObject = primitive, componentType = "Checkpoint" });
            }
            if (name.Equals("InGameTimer"))
            {
                primitive.AddComponent<InGameTimer>();
                EditorOp.Resolve<SceneDataHandler>().TimerManagerObj = primitive;
                EditorOp.Resolve<UILogicDisplayProcessor>().AddComponentIcon(new ComponentDisplayDock { componentGameObject = primitive, componentType = "InGameTimer" });
            }
        }

        private bool CanSpawn(string name)
        {
            if (name.Equals("InGameTimer"))
            {
                var timerObj = EditorOp.Resolve<SceneDataHandler>().TimerManagerObj;
                var canSpawn = !timerObj;
                return canSpawn;
            }
            return true;
        }

        public override void Draw()
        {
            //Nothing to draw
        }

        public override void Flush()
        {
            //Nothing to flush here
        }

        public override void Repaint()
        {
            //Nothing to re-paint
        }

        private void AddListenerEvent(Button button, Action callback)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { callback?.Invoke(); });
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }
    }
}
