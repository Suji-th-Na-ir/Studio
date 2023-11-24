using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using System.Linq;

namespace Terra.Studio
{
    public class ToolbarView : View, ITooltipManager
    {
        public event Action OnPublishRequested;

        private const string ADD_BUTTON_LOC = "AddButton";
        private const string PLAY_BUTTON_LOC = "PlayButton";
        private const string PUBLISH_BUTTON_LOC = "PublishButton";
        private const string SAVE_BUTTON_LOC = "SaveButton";
        private const string LOAD_BUTTON_LOC = "LoadButton";
        private const string MOVE_BUTTON_LOC = "MoveButton";
        private const string ROTATE_BUTTON_LOC = "RotateButton";
        private const string SCALE_BUTTON_LOC = "ScaleButton";
        private const string CYLINDER_PRIMITIVE_BUTTON_LOC = "cylinder_button";
        private const string SPHERE_PRIMITIVE_BUTTON_LOC = "sphere_button";
        private const string CUBE_PRIMITIVE_BUTTON_LOC = "cube_button";
        private const string PLANE_PRIMITIVE_BUTTON_LOC = "plane_button";
        private const string CHECKPOINT_PRIMITIVE_BUTTON_LOC = "checkpoint_button";
        private const string TIMER_BUTTON_LOC = "timer_button";
        private const string UNDO_BUTTON_LOC = "UndoButton";
        private const string REDO_BUTTON_LOC = "RedoButton";
        private const string SAVE_MESSAGE_TEXT_LOC = "SaveText";
        private const string LEFT_GROUP_LOC = "LeftGroup";

        private GameObject primitivePanel;
        private CanvasGroup[] canvasGroups;
        private TextMeshProUGUI saveTextField;
        private Button saveButton;
        private Button publishButton;
        private Button timerButton;

        public UISkin Skin => EditorOp.Resolve<RuntimeInspector>().Skin;

        public Canvas Canvas => EditorOp.Resolve<RuntimeInspector>().Canvas;

        public float TooltipDelay => 0.5f;

        public TooltipListener TooltipListener;

        private void Awake()
        {
            EditorOp.Register(this);
        }

        public override void Init()
        {
            TooltipListener = gameObject.AddComponent<TooltipListener>();
            TooltipListener.Initialize(this);
            
            SpawnPrimitivePrefab();
            var addButtonTr = Helper.FindDeepChild(transform, ADD_BUTTON_LOC);
            var playButtonTr = Helper.FindDeepChild(transform, PLAY_BUTTON_LOC);
            var publishButtonTr = Helper.FindDeepChild(transform, PUBLISH_BUTTON_LOC);
            var saveButtonTr = Helper.FindDeepChild(transform, SAVE_BUTTON_LOC);
            var loadButtonTr = Helper.FindDeepChild(transform, LOAD_BUTTON_LOC);
            var cylinderPrimitiveTr = Helper.FindDeepChild(
                transform,
                CYLINDER_PRIMITIVE_BUTTON_LOC
            );
            var spherePrimitiveTr = Helper.FindDeepChild(transform, SPHERE_PRIMITIVE_BUTTON_LOC);
            var cubePrimitiveTr = Helper.FindDeepChild(transform, CUBE_PRIMITIVE_BUTTON_LOC);
            var planePrimitiveTr = Helper.FindDeepChild(transform, PLANE_PRIMITIVE_BUTTON_LOC);
            var checkpointTr = Helper.FindDeepChild(transform, CHECKPOINT_PRIMITIVE_BUTTON_LOC);
            var timerTr = Helper.FindDeepChild(transform, TIMER_BUTTON_LOC);
            var moveButtonTr = Helper.FindDeepChild(transform, MOVE_BUTTON_LOC);
            var rotateButtonTr = Helper.FindDeepChild(transform, ROTATE_BUTTON_LOC);
            var scaleButtonTr = Helper.FindDeepChild(transform, SCALE_BUTTON_LOC);
            var undoButtonTr = Helper.FindDeepChild(transform, UNDO_BUTTON_LOC);
            var redoButtonTr = Helper.FindDeepChild(transform, REDO_BUTTON_LOC);
            saveTextField = Helper.FindDeepChild<TextMeshProUGUI>(transform, SAVE_MESSAGE_TEXT_LOC);

            var addButton = addButtonTr.GetComponent<Button>();
            AddListenerEvent(
                addButton,
                () =>
                {
                    var currentActiveState = primitivePanel.activeSelf;
                    primitivePanel.SetActive(!currentActiveState);
                }
            );

            var playButton = playButtonTr.GetComponent<Button>();
            AddListenerEvent(
                playButton,
                () =>
                {
                    playButton.interactable = false;
                    EditorOp
                        .Resolve<SceneDataHandler>()
                        .PrepareSceneDataToRuntime(() =>
                        {
                            EditorOp.Resolve<EditorSystem>().RequestSwitchState();
                        });
                }
            );

            publishButton = publishButtonTr.GetComponent<Button>();
            AddListenerEvent(
                publishButton,
                () =>
                {
                    SetPublishButtonActive(false);
                    if (saveTextField.text.Equals(SaveState.UnsavedChanges.GetStringValue()))
                    {
                        EditorOp.Resolve<SceneDataHandler>().Save(OnPublishRequested);
                    }
                    else
                    {
                        OnPublishRequested?.Invoke();
                    }
                }
            );

            var moveButton = moveButtonTr.GetComponent<Button>();
            AddListenerEvent(
                moveButton,
                () =>
                {
                    EditorOp
                        .Resolve<SelectionHandler>()
                        .SetWorkGizmoId(SelectionHandler.GizmoId.Move);
                }
            );

            var rotateButton = rotateButtonTr.GetComponent<Button>();
            AddListenerEvent(
                rotateButton,
                () =>
                {
                    EditorOp
                        .Resolve<SelectionHandler>()
                        .SetWorkGizmoId(SelectionHandler.GizmoId.Rotate);
                }
            );

            var scaleButton = scaleButtonTr.GetComponent<Button>();
            AddListenerEvent(
                scaleButton,
                () =>
                {
                    EditorOp
                        .Resolve<SelectionHandler>()
                        .SetWorkGizmoId(SelectionHandler.GizmoId.Scale);
                }
            );

            saveButton = saveButtonTr.GetComponent<Button>();
            AddListenerEvent(
                saveButton,
                () =>
                {
                    EditorOp.Resolve<SceneDataHandler>().Save();
                }
            );
            SetSaveMessage(true, SaveState.Empty);

            var cylinderButton = cylinderPrimitiveTr.GetComponent<Button>();
            AddListenerEvent(cylinderButton, CreateObject, PrimitiveType.Cylinder.ToString(), EditorObjectType.Default);

            var sphereButton = spherePrimitiveTr.GetComponent<Button>();
            AddListenerEvent(sphereButton, CreateObject, PrimitiveType.Sphere.ToString(), EditorObjectType.Default);

            var cubeButton = cubePrimitiveTr.GetComponent<Button>();
            AddListenerEvent(cubeButton, CreateObject, PrimitiveType.Cube.ToString(), EditorObjectType.Default);

            var planeButton = planePrimitiveTr.GetComponent<Button>();
            AddListenerEvent(planeButton, CreateObject, PrimitiveType.Plane.ToString(), EditorObjectType.Default);

            var checkpointButton = checkpointTr.GetComponent<Button>();
            AddListenerEvent(checkpointButton, CreateObject, "CheckPoint", EditorObjectType.Checkpoint);

            timerButton = timerTr.GetComponent<Button>();
            AddListenerEvent(timerButton, CreateObject, "InGameTimer", EditorObjectType.Timer);

            var undoButton = undoButtonTr.GetComponent<Button>();
            AddListenerEvent(undoButton, EditorOp.Resolve<IURCommand>().Undo);
            undoButton.interactable = false;
            EditorOp.Resolve<IURCommand>().OnUndoStackAvailable += (isPresent) =>
            {
                undoButton.interactable = isPresent;
            };

            var redoButton = redoButtonTr.GetComponent<Button>();
            AddListenerEvent(redoButton, EditorOp.Resolve<IURCommand>().Redo);
            redoButton.interactable = false;
            EditorOp.Resolve<IURCommand>().OnRedoStackAvailable += (isPresent) =>
            {
                redoButton.interactable = isPresent;
            };

            EditorOp.Resolve<SelectionHandler>().SelectionChanged += (_) =>
            {
                primitivePanel.SetActive(false);
            };

            var leftGroup = Helper.FindDeepChild<Transform>(transform, LEFT_GROUP_LOC);
            canvasGroups = leftGroup.GetComponentsInChildren<CanvasGroup>();
            EditorOp.Resolve<EditorSystem>().OnIncognitoEnabled += (isEnabled) =>
            {
                foreach (var canvasGroup in canvasGroups)
                {
                    canvasGroup.SetInteractive(!isEnabled);
                }
            };
        }

        public void ToggleInteractionOfGroup(string groupName, bool status)
        {
            var results = canvasGroups.Where(x => x.name.Contains(groupName)).Select(y => y);
            if (results.Count() == 0) return;
            results.First().SetInteractive(status);
        }

        private void AddListenerEvent<T, T1>(Button button, Action<T, T1> callback, T name, T1 type) where T1 : Enum
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                callback?.Invoke(name, type);
            });
        }

        public void CreateObject(string name, EditorObjectType type)
        {
            Transform cameraTransform = Camera.main.transform;
            Vector3 cameraPosition = cameraTransform.position;
            Vector3 spawnPosition = cameraPosition + cameraTransform.forward * 5;
            GameObject obj;
            if (type == EditorObjectType.Default)
            {
                var itemData = ((ResourceDB)SystemOp.Load(ResourceTag.ResourceDB)).GetItemDataForNearestName(name);
                obj = RuntimeWrappers.SpawnGameObject(itemData.ResourcePath, itemData);
            }
            else
            {
                EditorOp.Resolve<EditorEssentialsLoader>().Load(type, out obj);
            }
            obj.transform.position = spawnPosition;
            Snapshots.SpawnGameObjectSnapshot.CreateSnapshot(obj);
            EditorOp.Resolve<SelectionHandler>().DeselectAll();
            EditorOp.Resolve<RuntimeHierarchy>().Refresh();
            EditorOp.Resolve<SelectionHandler>().SelectObjectsInHierarchy(new List<Transform>() { obj.transform });
        }

        public void SetSaveMessage(bool setInteractable, SaveState state)
        {
            if (!string.IsNullOrEmpty(saveTextField.text) && state == SaveState.UnsavedChanges)
            {
                return;
            }
            if (state == SaveState.SavedToCloud)
            {
                CoroutineService.RunCoroutine(
                    () =>
                    {
                        SetSaveMessage(true, SaveState.Empty);
                    },
                    CoroutineService.DelayType.WaitForXSeconds,
                    2f
                );
            }
            var message = state.GetStringValue();
            saveButton.gameObject.SetActive(setInteractable);
            saveTextField.text = message;
        }

        public void SetPublishButtonActive(bool isInteractive)
        {
            if (!publishButton) return;
            publishButton.interactable = isInteractive;
        }

        public void SetTimerButtonInteractive(bool isInteractive)
        {
            if (!timerButton) return;
            timerButton.interactable = isInteractive;
        }

        private void SpawnPrimitivePrefab()
        {
            var resObj = EditorOp.Load<GameObject>("Prefabs/PrimitivesPanel");
            primitivePanel = Instantiate(resObj, transform);
            primitivePanel.SetActive(false);
        }

        private void AddListenerEvent(Button button, Action callback)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                callback?.Invoke();
            });
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }
    }
}
