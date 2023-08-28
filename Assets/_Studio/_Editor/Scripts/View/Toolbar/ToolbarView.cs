using System;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class ToolbarView : View
    {
        private const string PLAY_BUTTON_LOC = "PlayButton";
        private const string SAVE_BUTTON_LOC = "SaveButton";
        private const string LOAD_BUTTON_LOC = "LoadButton";
        private const string CYLINDER_PRIMITIVE_BUTTON_LOC = "cylinder_button";
        private const string SPHERE_PRIMITIVE_BUTTON_LOC = "sphere_button";
        private const string CUBE_PRIMITIVE_BUTTON_LOC = "cube_button";
        private const string PLANE_PRIMITIVE_BUTTON_LOC = "plane_button";
        private const string CHECKPOINT_PRIMITIVE_BUTTON_LOC = "checkpoint_button";

        private void Awake()
        {
            EditorOp.Register(this);
        }

        public override void Init()
        {
            var playButtonTr = Helper.FindDeepChild(transform, PLAY_BUTTON_LOC, true);
            var saveButtonTr = Helper.FindDeepChild(transform, SAVE_BUTTON_LOC, true);
            var loadButtonTr = Helper.FindDeepChild(transform, LOAD_BUTTON_LOC, true);
            var cylinderPrimitiveTr = Helper.FindDeepChild(transform, CYLINDER_PRIMITIVE_BUTTON_LOC, true);
            var spherePrimitiveTr = Helper.FindDeepChild(transform, SPHERE_PRIMITIVE_BUTTON_LOC, true);
            var cubePrimitiveTr = Helper.FindDeepChild(transform, CUBE_PRIMITIVE_BUTTON_LOC, true);
            var planePrimitiveTr = Helper.FindDeepChild(transform, PLANE_PRIMITIVE_BUTTON_LOC, true);
            var checkpointTr = Helper.FindDeepChild (transform, CHECKPOINT_PRIMITIVE_BUTTON_LOC, true);

            var playButton = playButtonTr.GetComponent<Button>();
            AddListenerEvent(playButton, () =>
            {
                EditorOp.Resolve<SceneDataHandler>().PrepareSceneDataToRuntime();
                EditorOp.Resolve<EditorSystem>().RequestSwitchState();
            });

            var saveButton = saveButtonTr.GetComponent<Button>();
            AddListenerEvent(saveButton, EditorOp.Resolve<SceneDataHandler>().Save);

            var cylinderButton = cylinderPrimitiveTr.GetComponent<Button>();
            AddListenerEvent(cylinderButton, CreatePrimitive, PrimitiveType.Cylinder);

            var sphereButton = spherePrimitiveTr.GetComponent<Button>();
            AddListenerEvent(sphereButton, CreatePrimitive, PrimitiveType.Sphere);

            var cubeButton = cubePrimitiveTr.GetComponent<Button>();
            AddListenerEvent(cubeButton, CreatePrimitive, PrimitiveType.Cube);

            var planeButton = planePrimitiveTr.GetComponent<Button>();
            AddListenerEvent(planeButton, CreatePrimitive, PrimitiveType.Plane);

            var checkpointButton = checkpointTr.GetComponent<Button> ();
            AddListenerEvent (checkpointButton, CreateCheckPoint, "CheckPoint");

        }

        private void AddListenerEvent<T>(Button button, Action<T> callback, T type)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { callback?.Invoke(type); });
        }

        public void CreateCheckPoint (string a_strPrefabName) {
            Transform cameraTransform = Camera.main.transform;
            Vector3 cameraPosition = cameraTransform.position;
            Vector3 spawnPosition = cameraPosition + cameraTransform.forward * 5;
            var itemData = ((ResourceDB)SystemOp.Load (ResourceTag.ResourceDB)).GetItemDataForNearestName (a_strPrefabName);
            var primitive = RuntimeWrappers.SpawnGameObject (itemData.ResourcePath, itemData);
            primitive.transform.position = spawnPosition;
            EditorOp.Resolve<SelectionHandler> ().OnSelectionChanged (primitive);
            EditorOp.Resolve<SelectionHandler> ().SelectObjectInHierarchy (primitive);
            primitive.AddComponent<RuntimeInspectorNamespace.Checkpoint> ();
        }

        public void CreatePrimitive(PrimitiveType type)
        {
            Transform cameraTransform = Camera.main.transform;
            Vector3 cameraPosition = cameraTransform.position;
            Vector3 spawnPosition = cameraPosition + cameraTransform.forward * 5;
            var itemData = ((ResourceDB)SystemOp.Load(ResourceTag.ResourceDB)).GetItemDataForNearestName(type.ToString());
            var primitive = RuntimeWrappers.SpawnGameObject(itemData.ResourcePath, itemData);
            primitive.transform.position = spawnPosition;
            EditorOp.Resolve<SelectionHandler>().OnSelectionChanged(primitive);
            EditorOp.Resolve<SelectionHandler>().SelectObjectInHierarchy(primitive);
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
