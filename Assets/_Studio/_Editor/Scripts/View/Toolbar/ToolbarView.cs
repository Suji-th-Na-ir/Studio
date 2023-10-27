using System;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using TMPro;

namespace Terra.Studio
{
    public class ToolbarView : View
    {
        private const string ADD_BUTTON_LOC = "AddButton";
        private const string PLAY_BUTTON_LOC = "PlayButton";
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

        private GameObject primitivePanel;
        private CanvasGroup canvasGroup;
        private TextMeshProUGUI saveTextField;
        private Button saveButton;

        private void Awake()
        {
            EditorOp.Register(this);
        }

        public override void Init()
        {
            SpawnPrimitivePrefab();
            var addButtonTr = Helper.FindDeepChild(transform, ADD_BUTTON_LOC);
            var playButtonTr = Helper.FindDeepChild(transform, PLAY_BUTTON_LOC);
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
                    EditorOp
                        .Resolve<SceneDataHandler>()
                        .PrepareSceneDataToRuntime(() =>
                        {
                            EditorOp.Resolve<EditorSystem>().RequestSwitchState();
                        });
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

            canvasGroup = GetComponentInChildren<CanvasGroup>();
            EditorOp.Resolve<EditorSystem>().OnIncognitoEnabled += (isEnabled) =>
            {
                canvasGroup.SetInteractive(!isEnabled);
            };
        }

        private void AddListenerEvent<T>(Button button, Action<T> callback, T type)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                callback?.Invoke(type);
            });
        }

        public void CreateObject(string name)
        {
            Transform cameraTransform = Camera.main.transform;
            Vector3 cameraPosition = cameraTransform.position;
            Vector3 spawnPosition = cameraPosition + cameraTransform.forward * 5;
            var itemData = (
                (ResourceDB)SystemOp.Load(ResourceTag.ResourceDB)
            ).GetItemDataForNearestName(name);
            var primitive = RuntimeWrappers.SpawnGameObject(itemData.ResourcePath, itemData);
            primitive.transform.position = spawnPosition;

            Snapshots.SpawnGameObjectSnapshot.CreateSnapshot(primitive);

            if (name.Equals("CheckPoint"))
            {
                AddCheckpointData(primitive);
            }

            if (name.Equals("InGameTimer"))
            {
                AddInGameTimerData(primitive);
            }

            EditorOp.Resolve<SelectionHandler>().DeselectAll();
            EditorOp.Resolve<SelectionHandler>().OnSelectionChanged(primitive);
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
            saveButton.interactable = setInteractable;
            saveTextField.text = message;
        }

        private void SpawnPrimitivePrefab()
        {
            var resObj = EditorOp.Load<GameObject>("Prefabs/PrimitivesPanel");
            primitivePanel = Instantiate(resObj, transform);
            primitivePanel.SetActive(false);
        }

        private void AddCheckpointData(GameObject go)
        {
            go.AddComponent<Checkpoint>();
        }

        private void AddInGameTimerData(GameObject go)
        {
            go.AddComponent<InGameTimer>();
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
