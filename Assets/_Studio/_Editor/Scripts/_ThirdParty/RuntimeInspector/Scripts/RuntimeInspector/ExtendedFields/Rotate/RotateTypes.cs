using System;
using System.Linq;
using UnityEngine;
using Terra.Studio;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections.Generic;
using Object = System.Object;
using UnityEngine.UI.ProceduralImage;

namespace RuntimeInspectorNamespace
{
    public class RotateTypes : MonoBehaviour
    {
        public RotationType myType;
        public Dropdown dirDropDown;
        public InputField[] ValuesPerAxis;
        public InputField speedInput = null;
        public InputField pauseInput = null;
        public InputField repeatInput = null;
        public Dropdown broadcastAt;
        public InputField customString;
        public Toggle canListenMultipleTimesToggle;

        [Header("Recorded Vector3 data")]
        public Button recordButton;
        public Button resetButton;
        public Image recordImageHolder;
        public Sprite recordImage;
        public Sprite saveImage;

        private Action[] valuesPerAxisActions;
        private Action dirAction;
        private Action speedAction;
        private Action pauseAction;
        private Action repeatAction;
        private Action broadcastAtAction;
        private Action customStringAction;
        private Action canListenMultipleTimesAction;
        private bool isRecording;

        [HideInInspector]
        public RotateField field = null;

        [SerializeField] private Text m_refBroadcastAtTitleText;
        [SerializeField] private Text m_refBroadcastTitleText;

        [SerializeField] private Text m_refMoveByTitleText;
        [SerializeField] private Text [] m_refMoveByAxisLabels;

        [SerializeField] private Text m_refSpeedTitleText;
        [SerializeField] private Text m_refPauseTitleText;
        [SerializeField] private Text m_refDirectionTitleText;
        [SerializeField] private Text m_refRepeatTitleText;

        private enum VariableTypes
        {
            DIRECTION_DROPDOWN,
            BROADCAST_AT,
            SPEED,
            PAUSE,
            REPEAT,
            BROADCAST_STRING,
            CAN_LISTEN_MULTIPLE_TIMES,
            RECORDED_VECTOR3
        }

        public void Setup()
        {
            LoadDefaultValues();
            SetActions();

            if (ValuesPerAxis?[0] != null)
            {
                ValuesPerAxis?[0].onValueChanged.AddListener(
                (value) =>
                {
                    var x = Helper.StringToFloat(value);
                    field.GetAtom().data.vector3.Set(Axis.X, x);
                    valuesPerAxisActions?[0]?.Invoke();
                });
                ValuesPerAxis?[0].onEndEdit.AddListener(
                (value) =>
                {
                    var x = Helper.StringToFloat(value);
                    var actualValue = (Vector3)((RotateComponentData)field.GetLastSubmittedValue()).vector3.Get();
                    if (x != actualValue.x)
                    {
                        UpdateUndoRedoStack("X In Vector3", actualValue, valuesPerAxisActions?[0]);
                    }
                });
            }

            if (ValuesPerAxis?[1] != null)
            {
                ValuesPerAxis?[1].onValueChanged.AddListener(
                (value) =>
                {
                    var y = Helper.StringToFloat(value);
                    field.GetAtom().data.vector3.Set(Axis.Y, y);
                    valuesPerAxisActions?[1]?.Invoke();
                });
                ValuesPerAxis?[1].onEndEdit.AddListener(
                (value) =>
                {
                    var y = Helper.StringToFloat(value);
                    var actualValue = (Vector3)((RotateComponentData)field.GetLastSubmittedValue()).vector3.Get();
                    if (y != actualValue.y)
                    {
                        UpdateUndoRedoStack("Y In Vector3", actualValue, valuesPerAxisActions?[1]);
                    }
                });
            }

            if (ValuesPerAxis?[2] != null)
            {
                ValuesPerAxis?[2].onValueChanged.AddListener(
                (value) =>
                {
                    var z = Helper.StringToFloat(value);
                    field.GetAtom().data.vector3.Set(Axis.Z, z);
                    valuesPerAxisActions?[2]?.Invoke();
                });
                ValuesPerAxis?[2].onEndEdit.AddListener(
                (value) =>
                {
                    var z = Helper.StringToFloat(value);
                    var actualValue = (Vector3)((RotateComponentData)field.GetLastSubmittedValue()).vector3.Get();
                    if (z != actualValue.z)
                    {
                        UpdateUndoRedoStack("Z In Vector3", actualValue, valuesPerAxisActions?[2]);
                    }
                });
            }

            if (dirDropDown != null)
            {
                dirDropDown.onValueChanged.AddListener((value) =>
                {
                    var dir = (Direction)value;
                    if (dir != field.GetAtom().data.direction)
                    {
                        field.GetAtom().data.direction = dir;
                        dirAction?.Invoke();
                        UpdateUndoRedoStack("Direction", dir, dirAction);
                    }
                });
            }
            if (speedInput != null)
            {
                speedInput.onValueChanged.AddListener((value) =>
                {
                    field.GetAtom().data.speed = Helper.StringToFloat(value);
                    speedAction?.Invoke();
                });
                speedInput.onEndEdit.AddListener((value) =>
                {
                    if (Helper.StringToFloat(value) != ((RotateComponentData)field.GetLastSubmittedValue()).speed)
                    {
                        UpdateUndoRedoStack("Speed", field.GetAtom().data.speed, speedAction);
                    }
                });
            }
            if (pauseInput != null)
            {
                pauseInput.onValueChanged.AddListener((value) =>
                {
                    field.GetAtom().data.pauseBetween = Helper.StringToFloat(value);
                    pauseAction?.Invoke();
                });
                speedInput.onEndEdit.AddListener((value) =>
                {
                    if (Helper.StringToFloat(value) != ((RotateComponentData)field.GetLastSubmittedValue()).pauseBetween)
                    {
                        UpdateUndoRedoStack("Pause between", field.GetAtom().data.pauseBetween, pauseAction);
                    }
                });
            }
            if (repeatInput != null)
            {
                repeatInput.onValueChanged.AddListener((value) =>
                {
                    field.GetAtom().data.repeat = Helper.StringInInt(value);
                    repeatAction?.Invoke();
                });
                repeatInput.onEndEdit.AddListener((value) =>
                {
                    if (Helper.StringToFloat(value) != ((RotateComponentData)field.GetLastSubmittedValue()).repeat)
                    {
                        UpdateUndoRedoStack("Repeat", field.GetAtom().data.repeat, repeatAction);
                    }
                });
            }
            if (customString != null)
            {
                customString.onValueChanged.AddListener((value) =>
                {
                    SetCustomString(value);
                });
                customString.onEndEdit.AddListener((value) =>
                {
                    if (value != ((RotateComponentData)field.GetLastSubmittedValue()).Broadcast)
                    {
                        UpdateUndoRedoStack("Broadcast", field.GetAtom().data.Broadcast, customStringAction);
                    }
                });
            }
            if (broadcastAt != null)
            {
                broadcastAt.onValueChanged.AddListener((value) =>
                {
                    var newValue = (BroadcastAt)value;
                    if (newValue != field.GetAtom().data.broadcastAt)
                    {
                        field.GetAtom().data.broadcastAt = (BroadcastAt)value;
                        broadcastAtAction?.Invoke();
                        UpdateUndoRedoStack("Broadcast At", field.GetAtom().data.broadcastAt, broadcastAtAction);
                    }
                });
            }
            if (canListenMultipleTimesToggle != null)
            {
                canListenMultipleTimesToggle.onValueChanged.AddListener((value) =>
                {
                    var newValue = value ? Listen.Always : Listen.Once;
                    if (newValue != field.GetAtom().data.listen)
                    {
                        field.GetAtom().data.listen = newValue;
                        canListenMultipleTimesAction?.Invoke();
                        UpdateUndoRedoStack("Can listen multiple times", field.GetAtom().data.listen, canListenMultipleTimesAction);
                    }
                });
            }

            SetupRecordSections();
        }

        private void UpdateUndoRedoStack(string variableName, object value, Action onValueChanged)
        {
            EditorOp.Resolve<IURCommand>().Record(
                    (field.GetLastSubmittedValue(), onValueChanged, variableName), ((object)field.GetAtom().data, onValueChanged, variableName),
                    $"{variableName} changed to: {value}",
                    (tuple) =>
                    {
                        var (value, onChanged, varName) = ((object, Action, string))tuple;
                        field.SetLastSubmittedValue(value);
                        var newValue = (RotateComponentData)value;
                        if (varName.ToLower().Equals("broadcast"))
                        {
                            SetCustomString(newValue.Broadcast);
                        }
                        field.GetAtom().data = newValue;
                        if (!varName.ToLower().Equals("broadcast"))
                        {
                            onChanged?.Invoke();
                        }
                        SetData(newValue);
                    });
            field.SetLastSubmittedValue(field.GetAtom().data);
        }

        private void SetCustomString(string _newString)
        {
            Atom.Rotate atom = field.GetAtom();
            atom.data.Broadcast = _newString;
            customStringAction?.Invoke();
        }

        private void UpdateVariablesForAll(VariableTypes _type, Object _value)
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selectedObjects.Count > 1)
            {
                foreach (var obj in selectedObjects)
                {
                    if (obj.TryGetComponent(out Rotate comp))
                    {
                        switch (_type)
                        {
                            case VariableTypes.DIRECTION_DROPDOWN:
                                comp.Type.data.direction = (Direction)_value;
                                break;
                            case VariableTypes.BROADCAST_AT:
                                comp.Type.data.broadcastAt = (BroadcastAt)_value;
                                break;
                            case VariableTypes.SPEED:
                                comp.Type.data.speed = (float)_value;
                                break;
                            case VariableTypes.PAUSE:
                                comp.Type.data.pauseBetween = (float)_value;
                                break;
                            case VariableTypes.REPEAT:
                                comp.Type.data.repeat = (int)_value;
                                break;
                            case VariableTypes.BROADCAST_STRING:
                                comp.OnBroadcastStringUpdated($"{_value}", comp.Type.data.Broadcast);
                                comp.Type.data.Broadcast = (string)_value;
                                break;
                            case VariableTypes.CAN_LISTEN_MULTIPLE_TIMES:
                                comp.Type.data.listen = (Listen)_value;
                                break;
                            case VariableTypes.RECORDED_VECTOR3:
                                comp.Type.data.vector3.Set(_value);
                                break;
                        }
                    }
                }
            }
        }

        public void LoadDefaultValues()
        {
            if (dirDropDown != null) { dirDropDown.AddOptions(Enum.GetNames(typeof(Direction)).ToList()); }
            if (broadcastAt != null) { broadcastAt.AddOptions(Enum.GetNames(typeof(BroadcastAt)).ToList()); }
            if (myType == RotationType.OscillateForever || myType == RotationType.IncrementallyRotateForever)
            {
                broadcastAt.ClearOptions();
                broadcastAt.AddOptions(new List<string>()
                {
                    BroadcastAt.Never.ToString(),
                    BroadcastAt.AtEveryPause.ToString()
                });
            }
        }

        public void SetData(RotateComponentData _data)
        {
            if (dirDropDown) dirDropDown.SetValueWithoutNotify((int)Enum.Parse(typeof(Direction), _data.direction.ToString()));
            if (broadcastAt) broadcastAt.SetValueWithoutNotify((int)Enum.Parse(typeof(BroadcastAt), _data.broadcastAt.ToString()));
            if (speedInput) speedInput.SetTextWithoutNotify(_data.speed.ToString());
            if (pauseInput) pauseInput.SetTextWithoutNotify(_data.pauseBetween.ToString());
            if (repeatInput) repeatInput.SetTextWithoutNotify(_data.repeat.ToString());
            if (customString) customString.SetTextWithoutNotify(_data.Broadcast);
            if (canListenMultipleTimesToggle) canListenMultipleTimesToggle.SetIsOnWithoutNotify(_data.listen == Listen.Always);
            ValuesPerAxis?[0].SetTextWithoutNotify(((Vector3)_data.vector3.Get()).x.ToString());
            ValuesPerAxis?[1].SetTextWithoutNotify(((Vector3)_data.vector3.Get()).y.ToString());
            ValuesPerAxis?[2].SetTextWithoutNotify(((Vector3)_data.vector3.Get()).z.ToString());
        }

        public void ApplySkin(UISkin Skin)
        {
            if (canListenMultipleTimesToggle) canListenMultipleTimesToggle.SetupToggeleSkin(Skin);
            if (speedInput) speedInput.SetupInputFieldSkin(Skin);
            if (pauseInput) pauseInput.SetupInputFieldSkin(Skin);
            if (repeatInput) repeatInput.SetupInputFieldSkin(Skin);
            if (dirDropDown) dirDropDown.SetSkinDropDownField(Skin);
            if (broadcastAt) broadcastAt.SetSkinDropDownField(Skin);
            if (customString) customString.SetupInputFieldSkin(Skin);
            for (int i = 0; i < ValuesPerAxis.Length; i++)
            {
                ValuesPerAxis[i].SetupInputFieldSkin(Skin);
                ValuesPerAxis [i].gameObject.GetComponent<BoundInputField> ().Skin = Skin;
                ValuesPerAxis [i].GetComponent<ProceduralImage> ().color = Skin.InputFieldNormalBackgroundColor;
            }
            m_refMoveByTitleText.SetSkinText (Skin);
            if (m_refBroadcastTitleText != null) m_refBroadcastTitleText.SetSkinText (Skin);
            if (m_refBroadcastAtTitleText != null) m_refBroadcastAtTitleText.SetSkinText (Skin);
            for (int i = 0; i < m_refMoveByAxisLabels.Length; i++) {
                m_refMoveByAxisLabels [i].SetSkinText (Skin);
            }
            if (m_refSpeedTitleText != null) m_refSpeedTitleText.SetSkinText (Skin);
            if (m_refPauseTitleText != null) m_refPauseTitleText.SetSkinText (Skin);
            if (m_refDirectionTitleText != null) m_refDirectionTitleText.SetSkinText (Skin);
            if (m_refRepeatTitleText != null) m_refRepeatTitleText.SetSkinText (Skin);
        }

        private void SetActions()
        {
            dirAction = () => { UpdateVariablesForAll(VariableTypes.DIRECTION_DROPDOWN, field.GetAtom().data.direction); };
            speedAction = () => { UpdateVariablesForAll(VariableTypes.SPEED, field.GetAtom().data.speed); };
            pauseAction = () => { UpdateVariablesForAll(VariableTypes.PAUSE, field.GetAtom().data.pauseBetween); };
            repeatAction = () => { UpdateVariablesForAll(VariableTypes.REPEAT, field.GetAtom().data.repeat); };
            broadcastAtAction = () => { UpdateVariablesForAll(VariableTypes.BROADCAST_AT, field.GetAtom().data.broadcastAt); };
            customStringAction = () => { UpdateVariablesForAll(VariableTypes.BROADCAST_STRING, field.GetAtom().data.Broadcast); };
            canListenMultipleTimesAction = () => { UpdateVariablesForAll(VariableTypes.CAN_LISTEN_MULTIPLE_TIMES, field.GetAtom().data.listen); };
            valuesPerAxisActions = new Action[3];
            valuesPerAxisActions[0] = () => { UpdateVariablesForAll(VariableTypes.RECORDED_VECTOR3, field.GetAtom().data.vector3.Get()); };
            valuesPerAxisActions[1] = () => { UpdateVariablesForAll(VariableTypes.RECORDED_VECTOR3, field.GetAtom().data.vector3.Get()); };
            valuesPerAxisActions[2] = () => { UpdateVariablesForAll(VariableTypes.RECORDED_VECTOR3, field.GetAtom().data.vector3.Get()); };
        }

        private void SetupRecordSections()
        {
            AddButtonListener(resetButton, OnResetButtonClicked);
            AddButtonListener(recordButton, OnRecordButtonClicked);
            field.GetAtom().data.vector3.OnModified = ToggleInteractivityOfResetButton;
            CheckAndToggleInteractivityOfResetButton();
        }

        private void AddButtonListener(Button button, Action action)
        {
            if (!button) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private void OnRecordButtonClicked()
        {
            field.GetAtom().data.vector3.ToggleGhostMode?.Invoke();
            isRecording = !isRecording;
            if (isRecording)
            {
                recordImageHolder.sprite = saveImage;
            }
            else
            {
                recordImageHolder.sprite = recordImage;
            }
            CheckAndToggleInteractivityOfResetButton();
        }

        private void OnResetButtonClicked()
        {
            field.GetAtom().data.vector3.Reset();
            ToggleInteractivityOfResetButton(false);
        }

        private void CheckAndToggleInteractivityOfResetButton()
        {
            var isInteractive = field.GetAtom().data.vector3?.IsValueModified?.Invoke() ?? false;
            ToggleInteractivityOfResetButton(isInteractive);
        }

        private void ToggleInteractivityOfResetButton(bool isInteractable)
        {
            resetButton.interactable = isInteractable;
        }
    }
}