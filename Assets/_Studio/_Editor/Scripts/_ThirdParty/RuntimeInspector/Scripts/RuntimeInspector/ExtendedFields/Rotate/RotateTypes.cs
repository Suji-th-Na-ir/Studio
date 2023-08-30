using TMPro;
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
       // public Dropdown axisDropDown;
        public Dropdown dirDropDown;

        public Toggle xAxis;
        public Toggle yAxis;
        public Toggle zAxis;

        public TMP_InputField degreesInput = null;
        public TMP_InputField speedInput = null;
        public TMP_InputField pauseInput = null;
        public TMP_InputField repeatInput = null;

        public Dropdown broadcastAt;
        public TMP_InputField broadcastInput = null;
        public Toggle canListenMultipleTimesToggle;

        [HideInInspector]
        public RotateField rotateField = null;

        private string guid;

        private void Awake()
        {
            guid = Guid.NewGuid().ToString("N");
        }

        public void Update()
        {
            if (broadcastInput != null && !String.IsNullOrEmpty(broadcastInput.text))
            {
                EditorOp.Resolve<DataProvider>().UpdateListenToTypes(guid, broadcastInput.text);
            }
        }
        
        private enum VariableTypes
        {
            X_AXIS,
            Y_AXIS,
            Z_AXIS,
            DIRECTION_DROPDOWN,
            BROADCAST_AT_DROPDOWN,
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
            if (xAxis != null) xAxis.onValueChanged.AddListener((value) =>
            {
                // data.Xaxis= value;
                rotateField.GetRotateAtom().data.Xaxis = value;
                UpdateVariablesForAll(VariableTypes.X_AXIS, value);
            });
            if (yAxis != null) yAxis.onValueChanged.AddListener((value) =>
            {
                rotateField.GetRotateAtom().data.Yaxis = value;
                UpdateVariablesForAll(VariableTypes.Y_AXIS, value);
            });

            if (zAxis != null) zAxis.onValueChanged.AddListener((value) =>
            {
                rotateField.GetRotateAtom().data.Zaxis = value;
                UpdateVariablesForAll(VariableTypes.Z_AXIS, value);
            });

            if (dirDropDown != null) dirDropDown.onValueChanged.AddListener((value) =>
            {
                rotateField.GetRotateAtom().data.direction = (Direction)value;
                UpdateVariablesForAll(VariableTypes.DIRECTION_DROPDOWN, value);
            });
            if (broadcastAt != null) broadcastAt.onValueChanged.AddListener((value) =>
            {
                rotateField.GetRotateAtom().data.broadcastAt = (BroadcastAt)value;
                UpdateVariablesForAll(VariableTypes.BROADCAST_AT_DROPDOWN, value);
            });
            if (degreesInput != null) degreesInput.onValueChanged.AddListener((value) =>
            {
                rotateField.GetRotateAtom().data.degrees =  Helper.StringToFloat(value);
                UpdateVariablesForAll(VariableTypes.DEGREES,  Helper.StringToFloat(value));
            });
            if (speedInput != null) speedInput.onValueChanged.AddListener((value) =>
            {
                rotateField.GetRotateAtom().data.speed = Helper.StringToFloat(value);
                UpdateVariablesForAll(VariableTypes.SPEED,  Helper.StringToFloat(value));
            });
            if (pauseInput != null) pauseInput.onValueChanged.AddListener((value) =>
            {
                rotateField.GetRotateAtom().data.pauseBetween = Helper.StringToFloat(value);
                UpdateVariablesForAll(VariableTypes.PAUSE,  Helper.StringToFloat(value));
            });
            if (repeatInput != null) repeatInput.onValueChanged.AddListener((value) =>
            {
                rotateField.GetRotateAtom().data.repeat = Helper.StringInInt(value);
                UpdateVariablesForAll(VariableTypes.REPEAT,  Helper.StringInInt(value));
            });
            if (broadcastInput != null) broadcastInput.onValueChanged.AddListener((value) =>
            {
                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(value, value, new ComponentDisplayDock() { componentGameObject = ((Atom.Rotate)rotateField.Value).referenceGO, componentType = typeof(Atom.Rotate).Name });
                rotateField.GetRotateAtom().data.broadcast = value;
                UpdateVariablesForAll(VariableTypes.BROADCAST_STRING,  value);
            });
            if (canListenMultipleTimesToggle != null) canListenMultipleTimesToggle.onValueChanged.AddListener((value) =>
            {
                rotateField.GetRotateAtom().data.listen = value ? Listen.Always : Listen.Once;
                UpdateVariablesForAll(VariableTypes.CAN_LISTEN_MULTIPLE_TIMES,  value);
            });
        }

        private void UpdateVariablesForAll(VariableTypes _type, Object _value)
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selectedObjects.Count > 1)
            {
                foreach (var obj in selectedObjects)
                {
                    if (obj.GetComponent<Rotate>() != null)
                    {
                        Rotate rotate = obj.GetComponent<Rotate>();
                        switch (_type)
                        {
                            case VariableTypes.X_AXIS:
                                rotate.Type.data.Xaxis = (bool)_value;
                                break;
                            case VariableTypes.Y_AXIS:
                                rotate.Type.data.Yaxis = (bool)_value;
                                break;
                            case VariableTypes.Z_AXIS:
                                rotate.Type.data.Zaxis = (bool)_value;
                                break;
                            case VariableTypes.DIRECTION_DROPDOWN:
                                rotate.Type.data.direction = (Direction)_value;
                                break;
                            case VariableTypes.BROADCAST_AT_DROPDOWN:
                                rotate.Type.data.broadcastAt = (BroadcastAt)_value;
                                break;
                            case VariableTypes.DEGREES:
                                rotate.Type.data.degrees = (float)_value;
                                break;
                            case VariableTypes.SPEED:
                                rotate.Type.data.speed = (float)_value;
                                break;
                            case VariableTypes.PAUSE:
                                rotate.Type.data.pauseBetween = (float)_value;
                                break;
                            case VariableTypes.REPEAT:
                                rotate.Type.data.repeat = (int)_value;
                                break;
                            case VariableTypes.BROADCAST_STRING:
                                rotate.Type.data.broadcast = (string)_value;
                                break;
                            case VariableTypes.CAN_LISTEN_MULTIPLE_TIMES:
                                rotate.Type.data.listen = (Listen)_value;
                                break;
                        }
                    }
                }
            }
        }
        
        private Axis GetAxis(string _value)
        {
            if (_value == Axis.X.ToString()) return Axis.X;
            else if (_value == Axis.Y.ToString()) return Axis.Y;
            else if (_value == Axis.Z.ToString()) return Axis.Z;
            return Axis.X;
        }

        private Direction GetDirection(string _value)
        {
            if (_value == Direction.Clockwise.ToString()) return Direction.Clockwise;
            else if (_value == Direction.AntiClockwise.ToString()) return Direction.AntiClockwise;
            return Direction.Clockwise;
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
            // axis 
          //  if (axisDropDown != null) { axisDropDown.AddOptions(Enum.GetNames(typeof(Axis)).ToList()); }

            // direction
            if (dirDropDown != null) { dirDropDown.AddOptions(Enum.GetNames(typeof(Direction)).ToList()); }

            // broadcast at 
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
            // if (axisDropDown) axisDropDown.SetValueWithoutNotify((int)Enum.Parse(typeof(Axis), _data.axis.ToString()));
            if (xAxis) xAxis.SetIsOnWithoutNotify(_data.Xaxis);
            if (yAxis) yAxis.SetIsOnWithoutNotify(_data.Yaxis);
            if (zAxis) zAxis.SetIsOnWithoutNotify(_data.Zaxis);
            if (dirDropDown) dirDropDown.SetValueWithoutNotify((int)Enum.Parse(typeof(Direction), _data.direction.ToString()));
            if (broadcastAt) broadcastAt.SetValueWithoutNotify((int)Enum.Parse(typeof(BroadcastAt), _data.broadcastAt.ToString()));
            if (degreesInput) degreesInput.SetTextWithoutNotify(_data.degrees.ToString());
            if (speedInput) speedInput.SetTextWithoutNotify(_data.speed.ToString());
            if (pauseInput) pauseInput.SetTextWithoutNotify(_data.pauseBetween.ToString());
            if (repeatInput) repeatInput.SetTextWithoutNotify(_data.repeat.ToString());
            if (broadcastInput) broadcastInput.SetTextWithoutNotify(_data.broadcast);
            if (canListenMultipleTimesToggle) canListenMultipleTimesToggle.SetIsOnWithoutNotify(_data.listen == Listen.Always);
        }
    }
}