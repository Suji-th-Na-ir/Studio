using System;
using System.Linq;
using UnityEngine;
using Terra.Studio;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace RuntimeInspectorNamespace
{
    public class TranslateTypes : MonoBehaviour
    {
        public InputField[] movebyInput;
        public InputField speedInput = null;
        public InputField repeatInput = null;
        public InputField pauseForInput = null;
        public InputField listenTo = null;
        public Dropdown broadcastAt;
        public InputField customString;
        public Toggle canListenMultipleTimesToggle;
        [HideInInspector]
        public TranslateField field = null;

        public void Setup()
        {
            LoadDefaultValues();
            movebyInput?[0].onValueChanged.AddListener(
                (value) =>
                {
                    field.GetAtom().data.moveBy.x = Helper.StringToFloat(value);
                    UpdateAllSelectedObjects("moveBy", field.GetAtom().data.moveBy);
                });
            movebyInput?[0].onEndEdit.AddListener(
                (value) =>
                {
                    if (Helper.StringToFloat(value) != ((TranslateComponentData)field.GetLastSubmittedValue()).moveBy.x)
                    {
                        UpdateUndoRedoStack("MoveBy X", field.GetAtom().data.moveBy);
                    }
                });
            movebyInput?[1].onValueChanged.AddListener(
                (value) =>
                {
                    field.GetAtom().data.moveBy.y = Helper.StringToFloat(value);
                    UpdateAllSelectedObjects("moveBy", field.GetAtom().data.moveBy);
                });
            movebyInput?[1].onEndEdit.AddListener(
                (value) =>
                {
                    if (Helper.StringToFloat(value) != ((TranslateComponentData)field.GetLastSubmittedValue()).moveBy.y)
                    {
                        UpdateUndoRedoStack("MoveBy Y", field.GetAtom().data.moveBy);
                    }
                });
            movebyInput?[2].onValueChanged.AddListener(
                (value) =>
                {
                    field.GetAtom().data.moveBy.z = Helper.StringToFloat(value);
                    UpdateAllSelectedObjects("moveBy", field.GetAtom().data.moveBy);
                });
            movebyInput?[2].onEndEdit.AddListener(
                (value) =>
                {
                    if (Helper.StringToFloat(value) != ((TranslateComponentData)field.GetLastSubmittedValue()).moveBy.z)
                    {
                        UpdateUndoRedoStack("MoveBy Z", field.GetAtom().data.moveBy);
                    }
                });
            if (speedInput != null)
            {
                speedInput.onValueChanged.AddListener((value) =>
                {
                    field.GetAtom().data.speed = Helper.StringToFloat(value);
                    UpdateAllSelectedObjects("speed", field.GetAtom().data.speed);
                });
                speedInput.onEndEdit.AddListener((value) =>
                {
                    if (Helper.StringToFloat(value) != ((TranslateComponentData)field.GetLastSubmittedValue()).speed)
                    {
                        UpdateUndoRedoStack("Speed", field.GetAtom().data.speed);
                    }
                });
            }
            if (pauseForInput != null)
            {
                pauseForInput.onValueChanged.AddListener((value) =>
                {
                    field.GetAtom().data.pauseFor = Helper.StringToFloat(value);
                    UpdateAllSelectedObjects("pauseFor", field.GetAtom().data.pauseFor);
                });
                pauseForInput.onEndEdit.AddListener((value) =>
                {
                    if (Helper.StringToFloat(value) != ((TranslateComponentData)field.GetLastSubmittedValue()).pauseFor)
                    {
                        UpdateUndoRedoStack("PauseFor", field.GetAtom().data.pauseFor);
                    }
                });
            }
            if (repeatInput != null)
            {
                repeatInput.onValueChanged.AddListener((value) =>
                {
                    field.GetAtom().data.repeat = Helper.StringInInt(value);
                    UpdateAllSelectedObjects("repeat", field.GetAtom().data.repeat);
                });
                repeatInput.onEndEdit.AddListener((value) =>
                {
                    if (Helper.StringInInt(value) != ((TranslateComponentData)field.GetLastSubmittedValue()).repeat)
                    {
                        UpdateUndoRedoStack("Repeat", field.GetAtom().data.repeat);
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
                    if (value != ((TranslateComponentData)field.GetLastSubmittedValue()).broadcast)
                    {
                        UpdateUndoRedoStack("Broadcast", field.GetAtom().data.broadcast);
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
                        UpdateAllSelectedObjects("broadcastAt", field.GetAtom().data.broadcastAt);
                        UpdateUndoRedoStack("BroadcastAt", field.GetAtom().data.broadcastAt);
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
                        UpdateAllSelectedObjects("listen", field.GetAtom().data.listen);
                        UpdateUndoRedoStack("Listen", field.GetAtom().data.listen);
                    }
                });
            }
        }

        private void UpdateUndoRedoStack(string variableName, object value)
        {
            Debug.Log($"Updating stack: {variableName} | Value: {value}");
            EditorOp.Resolve<IURCommand>().Record(
                    field.GetLastSubmittedValue(), field.GetAtom().data,
                    $"{variableName} changed to: {value}",
                    (value) =>
                    {
                        field.SetLastSubmittedValue(value);
                        var newValue = (TranslateComponentData)value;
                        field.GetAtom().data = newValue;
                        SetData(newValue);
                    });
            field.SetLastSubmittedValue(field.GetAtom().data);
        }

        private void SetCustomString(string _newString)
        {
            Atom.Translate atom = field.GetAtom();
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(_newString,
                atom.data.broadcast,
                new ComponentDisplayDock
                {
                    componentGameObject = atom.target,
                    componentType = atom.componentType
                });
            atom.data.broadcast = _newString;
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
                    var typeValue = (Atom.Translate)typeField.GetValue(translate);
                    var dataField = typeValue.GetType().GetField("data", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    var dataValue = (TranslateComponentData)dataField.GetValue(typeValue);
                    var targetValue = dataValue.GetType().GetField(varName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                    if (varName == "broadcast")
                    {
                        EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(value.ToString(), targetValue.GetValue(dataValue).ToString(), new ComponentDisplayDock() { componentGameObject = obj, componentType = typeof(Atom.Translate).Name });
                    }
                    targetValue.SetValueDirect(__makeref(dataValue), value);
                    dataField.SetValue(typeValue, dataValue);
                    typeField.SetValue(translate, typeValue);
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
            movebyInput?[0].SetTextWithoutNotify(_data.moveBy.x.ToString());
            movebyInput?[1].SetTextWithoutNotify(_data.moveBy.y.ToString());
            movebyInput?[2].SetTextWithoutNotify(_data.moveBy.z.ToString());
            if (customString) customString.SetTextWithoutNotify(_data.broadcast);
            if (canListenMultipleTimesToggle) canListenMultipleTimesToggle.SetIsOnWithoutNotify(_data.listen == Listen.Always);
        }

        public void ApplySkin(UISkin skin)
        {
            canListenMultipleTimesToggle?.SetupToggeleSkin(skin);
            broadcastAt?.SetSkinDropDownField(skin);
            for (int i = 0; i < movebyInput.Length; i++)
            {
                movebyInput[i]?.SetupInputFieldSkin(skin);
            }
            speedInput?.SetupInputFieldSkin(skin);
            pauseForInput?.SetupInputFieldSkin(skin);
            repeatInput?.SetupInputFieldSkin(skin);
            listenTo?.SetupInputFieldSkin(skin);
            customString?.SetupInputFieldSkin(skin);
        }
    }
}