using System;
using System.Linq;
using UnityEngine;
using Terra.Studio;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections.Generic;
using Object = System.Object;

namespace RuntimeInspectorNamespace
{
    public class RotateTypes : MonoBehaviour
    {
        public RotationType myType;
        public Dropdown dirDropDown;
        public Toggle xAxis;
        public Toggle yAxis;
        public Toggle zAxis;
        public InputField degreesInput = null;
        public InputField speedInput = null;
        public InputField pauseInput = null;
        public InputField repeatInput = null;
        public Dropdown broadcastAt;
        public InputField customString;
        public Toggle canListenMultipleTimesToggle;

        private Action dirAction;
        private Action xAction;
        private Action yAction;
        private Action zAction;
        private Action degressAction;
        private Action speedAction;
        private Action pauseAction;
        private Action repeatAction;
        private Action broadcastAtAction;
        private Action customStringAction;
        private Action canListenMultipleTimesAction;

        [HideInInspector] public RotateField field = null;


        private enum VariableTypes
        {
            X_AXIS,
            Y_AXIS,
            Z_AXIS,
            DIRECTION_DROPDOWN,
            BROADCAST_AT,
            DEGREES,
            SPEED,
            PAUSE,
            REPEAT,
            BROADCAST_STRING,
            CAN_LISTEN_MULTIPLE_TIMES
        }

        public void Setup()
        {
            LoadDefaultValues();
            SetActions();
            if (xAxis != null)
            {
                xAxis.onValueChanged.AddListener((value) =>
                {
                    if (value != field.GetAtom().data.Xaxis)
                    {
                        field.GetAtom().data.Xaxis = value;
                        xAction?.Invoke();
                        UpdateUndoRedoStack("Rotate In X", value, xAction);
                    }
                });
            }
            if (yAxis != null)
            {
                yAxis.onValueChanged.AddListener((value) =>
                {
                    if (value != field.GetAtom().data.Yaxis)
                    {
                        field.GetAtom().data.Yaxis = value;
                        yAction?.Invoke();
                        UpdateUndoRedoStack("Rotate In Y", value, yAction);
                    }
                });
            }
            if (zAxis != null)
            {
                zAxis.onValueChanged.AddListener((value) =>
                {
                    if (value != field.GetAtom().data.Zaxis)
                    {
                        field.GetAtom().data.Zaxis = value;
                        zAction?.Invoke();
                        UpdateUndoRedoStack("Rotate In Z", value, zAction);
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
            if (degreesInput != null)
            {
                degreesInput.onValueChanged.AddListener((value) =>
                {
                    field.GetAtom().data.degrees = Helper.StringToFloat(value);
                    degressAction?.Invoke();
                });
                degreesInput.onEndEdit.AddListener((value) =>
                {
                    if (Helper.StringToFloat(value) != ((RotateComponentData)field.GetLastSubmittedValue()).degrees)
                    {
                        UpdateUndoRedoStack("Degrees", field.GetAtom().data.degrees, degressAction);
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
                            case VariableTypes.X_AXIS:
                                comp.Type.data.Xaxis = (bool)_value;
                                break;
                            case VariableTypes.Y_AXIS:
                                comp.Type.data.Yaxis = (bool)_value;
                                break;
                            case VariableTypes.Z_AXIS:
                                comp.Type.data.Zaxis = (bool)_value;
                                break;
                            case VariableTypes.DIRECTION_DROPDOWN:
                                comp.Type.data.direction = (Direction)_value;
                                break;
                            case VariableTypes.BROADCAST_AT:
                                comp.Type.data.broadcastAt = (BroadcastAt)_value;
                                break;
                            case VariableTypes.DEGREES:
                                comp.Type.data.degrees = (float)_value;
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
                                comp.OnBroadcastUpdated?.Invoke($"{_value}", comp.Type.data.Broadcast);
                                comp.Type.data.Broadcast = (string)_value;
                                break;
                            case VariableTypes.CAN_LISTEN_MULTIPLE_TIMES:
                                comp.Type.data.listen = (Listen)_value;
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
                    BroadcastAt.AtEveryInterval.ToString()
                });
            }
        }

        public void SetData(RotateComponentData _data)
        {
            if (xAxis) xAxis.SetIsOnWithoutNotify(_data.Xaxis);
            if (yAxis) yAxis.SetIsOnWithoutNotify(_data.Yaxis);
            if (zAxis) zAxis.SetIsOnWithoutNotify(_data.Zaxis);
            if (dirDropDown) dirDropDown.SetValueWithoutNotify((int)Enum.Parse(typeof(Direction), _data.direction.ToString()));
            if (broadcastAt) broadcastAt.SetValueWithoutNotify((int)Enum.Parse(typeof(BroadcastAt), _data.broadcastAt.ToString()));
            if (degreesInput) degreesInput.SetTextWithoutNotify(_data.degrees.ToString());
            if (speedInput) speedInput.SetTextWithoutNotify(_data.speed.ToString());
            if (pauseInput) pauseInput.SetTextWithoutNotify(_data.pauseBetween.ToString());
            if (repeatInput) repeatInput.SetTextWithoutNotify(_data.repeat.ToString());
            if (customString) customString.SetTextWithoutNotify(_data.Broadcast);
            if (canListenMultipleTimesToggle) canListenMultipleTimesToggle.SetIsOnWithoutNotify(_data.listen == Listen.Always);
        }

        public void ApplySkin(UISkin Skin)
        {
            xAxis?.SetupToggeleSkin(Skin);
            yAxis?.SetupToggeleSkin(Skin);
            zAxis?.SetupToggeleSkin(Skin);
            canListenMultipleTimesToggle?.SetupToggeleSkin(Skin);
            degreesInput?.SetupInputFieldSkin(Skin);
            speedInput?.SetupInputFieldSkin(Skin);
            pauseInput?.SetupInputFieldSkin(Skin);
            repeatInput?.SetupInputFieldSkin(Skin);
            dirDropDown?.SetSkinDropDownField(Skin);
            broadcastAt?.SetSkinDropDownField(Skin);
            customString?.SetupInputFieldSkin(Skin);
        }

        private void SetActions()
        {
            dirAction = () => { UpdateVariablesForAll(VariableTypes.DIRECTION_DROPDOWN, field.GetAtom().data.direction); };
            xAction = () => { UpdateVariablesForAll(VariableTypes.X_AXIS, field.GetAtom().data.Xaxis); };
            yAction = () => { UpdateVariablesForAll(VariableTypes.Y_AXIS, field.GetAtom().data.Yaxis); };
            zAction = () => { UpdateVariablesForAll(VariableTypes.Z_AXIS, field.GetAtom().data.Zaxis); };
            degressAction = () => { UpdateVariablesForAll(VariableTypes.DEGREES, field.GetAtom().data.degrees); };
            speedAction = () => { UpdateVariablesForAll(VariableTypes.SPEED, field.GetAtom().data.speed); };
            pauseAction = () => { UpdateVariablesForAll(VariableTypes.PAUSE, field.GetAtom().data.pauseBetween); };
            repeatAction = () => { UpdateVariablesForAll(VariableTypes.REPEAT, field.GetAtom().data.repeat); };
            broadcastAtAction = () => { UpdateVariablesForAll(VariableTypes.BROADCAST_AT, field.GetAtom().data.broadcastAt); };
            customStringAction = () => { UpdateVariablesForAll(VariableTypes.BROADCAST_STRING, field.GetAtom().data.Broadcast); };
            canListenMultipleTimesAction = () => { UpdateVariablesForAll(VariableTypes.CAN_LISTEN_MULTIPLE_TIMES, field.GetAtom().data.listen); };
        }
    }
}