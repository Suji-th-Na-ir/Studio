using System;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.IO;

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

            var playButton = playButtonTr.GetComponent<Button>();
            AddListenerEvent(playButton, () =>
            {
                var scene = SceneExporter.ExportJson();
                SystemOp.Resolve<CrossSceneDataHolder>().Set(scene);
                EditorOp.Resolve<EditorSystem>().RequestSwitchState();
            });

            var saveButton = saveButtonTr.GetComponent<Button>();
            AddListenerEvent(saveButton, EditorOp.Resolve<EditorSystem>().RequestSaveScene);

            var loadButton = loadButtonTr.GetComponent<Button>();
            AddListenerEvent(loadButton, EditorOp.Resolve<EditorSystem>().RequestLoadScene);

            var cylinderButton = cylinderPrimitiveTr.GetComponent<Button>();
            AddListenerEvent(cylinderButton, CreatePrimitive, PrimitiveType.Cylinder);

            var sphereButton = spherePrimitiveTr.GetComponent<Button>();
            AddListenerEvent(sphereButton, CreatePrimitive, PrimitiveType.Sphere);

            var cubeButton = cubePrimitiveTr.GetComponent<Button>();
            AddListenerEvent(cubeButton, CreatePrimitive, PrimitiveType.Cube);

            var planeButton = planePrimitiveTr.GetComponent<Button>();
            AddListenerEvent(planeButton, CreatePrimitive, PrimitiveType.Plane);

        }

        private void AddListenerEvent<T>(Button button, Action<T> callback, T type)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { callback?.Invoke(type); });
        }

        public void CreatePrimitive(PrimitiveType type)
        {
            Transform cameraTransform = Camera.main.transform;
            Vector3 cameraPosition = cameraTransform.position;

            // Calculate the spawn position by adding the forward direction scaled by spawnDistance
            Vector3 spawnPosition = cameraPosition + cameraTransform.forward * 5;
            var path = ResourceDB.GetStudioAsset(type.ToString()).ShortPath;
            var primitive = RuntimeWrappers.SpawnGameObject(path);
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
