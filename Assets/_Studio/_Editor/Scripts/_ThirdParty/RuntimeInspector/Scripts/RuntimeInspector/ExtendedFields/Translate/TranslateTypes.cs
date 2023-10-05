using System;
using System.Linq;
using UnityEngine;
using Terra.Studio;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections.Generic;
using UnityEngine.UI.ProceduralImage;

namespace RuntimeInspectorNamespace
{
    public class TranslateTypes : MonoBehaviour
    {
        public InputField[] movebyInput;
        public InputField speedInput = null;
        public InputField repeatInput = null;
        public InputField pauseForInput = null;
        public Dropdown broadcastAt;
        public InputField customString;
        public Toggle canListenMultipleTimesToggle;
        public Button recordButton;
        public Button resetButton;
        public Image recordImageHolder;
        public Sprite recordImage;
        public Sprite saveImage;

        private Action[] movebyActions;
        private Action speedAction;
        private Action repeatAction;
        private Action pauseAction;
        private Action broadcastAtAction;
        private Action customStringAction;
        private Action canListenMultipleAction;
        private bool isRecording;

        [SerializeField] private Text m_refBroadcastAtTitleText;
        [SerializeField] private Text m_refBroadcastTitleText;

        [SerializeField] private Text m_refMoveByTitleText;
        [SerializeField] private Text[] m_refMoveByAxisLabels;

        [HideInInspector]
        public TranslateField field = null;

        public void Setup()
        {
            LoadDefaultValues();
            SetActions();
            movebyInput?[0].onValueChanged.AddListener(
                (value) =>
                {
                    var x = Helper.StringToFloat(value);
                    field.GetAtom().data.recordedVector3.Set(Axis.X, x);
                    movebyActions?[0]?.Invoke();
                });
            movebyInput?[0].onEndEdit.AddListener(
                (value) =>
                {
                    if (Helper.StringToFloat(value) != ((Vector3)((TranslateComponentData)field.GetLastSubmittedValue()).recordedVector3.Get()).x)
                    {
                        UpdateUndoRedoStack("MoveBy X", field.GetAtom().data.recordedVector3.Get(), movebyActions?[0]);
                    }
                });
            movebyInput?[1].onValueChanged.AddListener(
                (value) =>
                {
                    var y = Helper.StringToFloat(value);
                    field.GetAtom().data.recordedVector3.Set(Axis.Y, y);
                    movebyActions?[1]?.Invoke();
                });
            movebyInput?[1].onEndEdit.AddListener(
                (value) =>
                {
                    if (Helper.StringToFloat(value) != ((Vector3)((TranslateComponentData)field.GetLastSubmittedValue()).recordedVector3.Get()).y)
                    {
                        UpdateUndoRedoStack("MoveBy Y", field.GetAtom().data.recordedVector3.Get(), movebyActions?[1]);
                    }
                });
            movebyInput?[2].onValueChanged.AddListener(
                (value) =>
                {
                    var z = Helper.StringToFloat(value);
                    field.GetAtom().data.recordedVector3.Set(Axis.Z, z);
                    movebyActions?[2]?.Invoke();
                });
            movebyInput?[2].onEndEdit.AddListener(
                (value) =>
                {
                    if (Helper.StringToFloat(value) != ((Vector3)((TranslateComponentData)field.GetLastSubmittedValue()).recordedVector3.Get()).z)
                    {
                        UpdateUndoRedoStack("MoveBy Z", field.GetAtom().data.recordedVector3.Get(), movebyActions?[2]);
                    }
                });
            if (speedInput != null)
            {
                speedInput.onValueChanged.AddListener((value) =>
                {
                    field.GetAtom().data.speed = Helper.StringToFloat(value);
                    speedAction?.Invoke();
                });
                speedInput.onEndEdit.AddListener((value) =>
                {
                    if (Helper.StringToFloat(value) != ((TranslateComponentData)field.GetLastSubmittedValue()).speed)
                    {
                        UpdateUndoRedoStack("Speed", field.GetAtom().data.speed, speedAction);
                    }
                });
            }
            if (pauseForInput != null)
            {
                pauseForInput.onValueChanged.AddListener((value) =>
                {
                    field.GetAtom().data.pauseFor = Helper.StringToFloat(value);
                    pauseAction?.Invoke();
                });
                pauseForInput.onEndEdit.AddListener((value) =>
                {
                    if (Helper.StringToFloat(value) != ((TranslateComponentData)field.GetLastSubmittedValue()).pauseFor)
                    {
                        UpdateUndoRedoStack("PauseFor", field.GetAtom().data.pauseFor, pauseAction);
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
                    if (Helper.StringInInt(value) != ((TranslateComponentData)field.GetLastSubmittedValue()).repeat)
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
                    if (value != ((TranslateComponentData)field.GetLastSubmittedValue()).Broadcast)
                    {
                        UpdateUndoRedoStack("Broadcast", field.GetAtom().data.Broadcast, customStringAction);
                    }
                });
            }
            if (broadcastAt != null)
            {
                broadcastAt.onValueChanged.AddListener((value) =>
                {
                    var broadcastAtTemp = GetBroadcastAt(broadcastAt.options[value].text);
                    if (broadcastAtTemp != field.GetAtom().data.broadcastAt)
                    {
                        field.GetAtom().data.broadcastAt = broadcastAtTemp;
                        broadcastAtAction?.Invoke();
                        UpdateUndoRedoStack("BroadcastAt", field.GetAtom().data.broadcastAt, broadcastAtAction);
                    }
                });
            }
            if (canListenMultipleTimesToggle != null)
            {
                canListenMultipleTimesToggle.onValueChanged.AddListener((value) =>
                {
                    var listenTemp = value ? Listen.Always : Listen.Once;
                    if (listenTemp != field.GetAtom().data.listen)
                    {
                        field.GetAtom().data.listen = value ? Listen.Always : Listen.Once;
                        canListenMultipleAction?.Invoke();
                        UpdateUndoRedoStack("Listen", field.GetAtom().data.listen, canListenMultipleAction);
                    }
                });
            }
            SetupRecordSections();
        }

        private void SetupRecordSections()
        {
            AddButtonListener(resetButton, OnResetButtonClicked);
            AddButtonListener(recordButton, OnRecordButtonClicked);
            field.GetAtom().data.recordedVector3.OnModified = ToggleInteractivityOfResetButton;
            CheckAndToggleInteractivityOfResetButton();
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
                        var newValue = (TranslateComponentData)value;
                        if (varName.ToLower().Equals("broadcast"))
                        {
                            SetCustomString(newValue.Broadcast);
                        }
                        var translate = field.GetAtom();
                        translate.data = newValue;
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
            Atom.Translate atom = field.GetAtom();
            atom.data.Broadcast = _newString;
            customStringAction?.Invoke();
        }

        private void UpdateAllSelectedObjects(string varName, object value)
        {
            var type = typeof(Translate);
            var typeField = type.GetField("Type", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            foreach (var obj in selectedObjects)
            {
                if (obj.TryGetComponent(out Translate translate))
                {
                    if (varName.Equals("broadcast"))
                    {
                        var newString = value == null ? string.Empty : (string)value;
                        var oldString = translate.Type.data.Broadcast ?? string.Empty;
                        translate.OnBroadcastStringUpdated(newString, oldString);
                        translate.Type.data.Broadcast = newString;
                    }
                    else
                    {
                        var typeValue = (Atom.Translate)typeField.GetValue(translate);
                        var dataField = typeValue.GetType().GetField("data", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        var dataValue = (TranslateComponentData)dataField.GetValue(typeValue);
                        var targetValue = dataValue.GetType().GetField(varName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                        if (targetValue != null)
                        {
                            targetValue.SetValueDirect(__makeref(dataValue), value);
                            dataField.SetValue(typeValue, dataValue);
                            typeField.SetValue(translate, typeValue);
                        }
                    }
                }
            }
        }

        private BroadcastAt GetBroadcastAt(string _value)
        {
            if (_value == BroadcastAt.End.ToString()) return BroadcastAt.End;
            else if (_value == BroadcastAt.Never.ToString()) return BroadcastAt.Never;
            else if (_value == BroadcastAt.AtEveryInterval.ToString()) return BroadcastAt.AtEveryInterval;
            return BroadcastAt.Never;
        }

        public void LoadDefaultValues()
        {
            if (broadcastAt) { broadcastAt.AddOptions(Enum.GetNames(typeof(BroadcastAt)).ToList()); }

            if (customString) { customString.text = ""; }
        }

        public void SetData(TranslateComponentData _data)
        {
            if (broadcastAt != null) broadcastAt.SetValueWithoutNotify((int)Enum.Parse(typeof(BroadcastAt), _data.broadcastAt.ToString()));
            if (pauseForInput != null) pauseForInput.SetTextWithoutNotify(_data.pauseFor.ToString());
            if (speedInput != null) speedInput.SetTextWithoutNotify(_data.speed.ToString());
            if (repeatInput != null) repeatInput.SetTextWithoutNotify(_data.repeat.ToString());
            movebyInput?[0].SetTextWithoutNotify(((Vector3)_data.recordedVector3.Get()).x.ToString());
            movebyInput?[1].SetTextWithoutNotify(((Vector3)_data.recordedVector3.Get()).y.ToString());
            movebyInput?[2].SetTextWithoutNotify(((Vector3)_data.recordedVector3.Get()).z.ToString());
            if (customString) customString.SetTextWithoutNotify(_data.Broadcast);
            if (canListenMultipleTimesToggle) canListenMultipleTimesToggle.SetIsOnWithoutNotify(_data.listen == Listen.Always);
        }

        public void ApplySkin(UISkin skin)
        {
            canListenMultipleTimesToggle.SetupToggeleSkin(skin);
            if (broadcastAt != null) broadcastAt.SetSkinDropDownField(skin);
            for (int i = 0; i < movebyInput.Length; i++)
            {
                movebyInput[i].SetupInputFieldSkin(skin);
                movebyInput[i].gameObject.GetComponent<BoundInputField>().Skin = skin;
                movebyInput [i].GetComponent<ProceduralImage> ().color = skin.InputFieldNormalBackgroundColor;
            }
            if (speedInput != null) speedInput.SetupInputFieldSkin(skin);
            if (pauseForInput != null) pauseForInput.SetupInputFieldSkin(skin);
            if (repeatInput != null) repeatInput.SetupInputFieldSkin(skin);
            m_refMoveByTitleText.SetSkinText (skin);
            if (m_refBroadcastTitleText != null) m_refBroadcastTitleText.SetSkinText (skin);
            if (m_refBroadcastAtTitleText != null) m_refBroadcastAtTitleText.SetSkinText (skin);
            for (int i = 0; i < m_refMoveByAxisLabels.Length; i++) {
                    m_refMoveByAxisLabels [i].SetSkinText (skin);
            }
        }

        private void SetActions()
        {
            movebyActions = new Action[3];
            movebyActions[0] = () => { UpdateAllSelectedObjects("moveBy", field.GetAtom().data.recordedVector3.Get()); };
            movebyActions[1] = () => { UpdateAllSelectedObjects("moveBy", field.GetAtom().data.recordedVector3.Get()); };
            movebyActions[2] = () => { UpdateAllSelectedObjects("moveBy", field.GetAtom().data.recordedVector3.Get()); };
            speedAction = () => { UpdateAllSelectedObjects("speed", field.GetAtom().data.speed); };
            repeatAction = () => { UpdateAllSelectedObjects("repeat", field.GetAtom().data.repeat); };
            pauseAction = () => { UpdateAllSelectedObjects("pauseFor", field.GetAtom().data.pauseFor); };
            broadcastAtAction = () => { UpdateAllSelectedObjects("broadcastAt", field.GetAtom().data.broadcastAt); };
            customStringAction = () => { UpdateAllSelectedObjects("broadcast", field.GetAtom().data.Broadcast); };
            canListenMultipleAction = () => { UpdateAllSelectedObjects("listen", field.GetAtom().data.listen); };
        }

        private void AddButtonListener(Button button, Action action)
        {
            if (!button) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private void OnRecordButtonClicked()
        {
            field.GetAtom().data.recordedVector3.ToggleGhostMode?.Invoke();
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
            field.GetAtom().data.recordedVector3.Reset();
            ToggleInteractivityOfResetButton(false);
        }

        private void CheckAndToggleInteractivityOfResetButton()
        {
            var isInteractive = field.GetAtom().data.recordedVector3?.IsValueModified?.Invoke() ?? false;
            ToggleInteractivityOfResetButton(isInteractive);
        }

        private void ToggleInteractivityOfResetButton(bool isInteractable)
        {
            resetButton.interactable = isInteractable;
        }
    }
}