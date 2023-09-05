using System;
using System.Linq;
using UnityEngine;
using Terra.Studio;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections.Generic;
using UnityEngine.Serialization;
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

        public InputField degreesInput = null;
        public InputField speedInput = null;
        public InputField pauseInput = null;
        public InputField repeatInput = null;

        public Dropdown broadcastAt;
        
        public Dropdown broadcastType;
        public InputField customString;
        
        public InputField listenInput;
        public Toggle canListenMultipleTimesToggle;

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
        
        
        public void RefreshUI()
        {
            if(broadcastType)
                ShowCustomStringInput(broadcastType.value);
        }
        
        public void Setup()
        {
            LoadDefaultValues();
            if (xAxis != null) xAxis.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.Xaxis = value;
                UpdateVariablesForAll(VariableTypes.X_AXIS, value);
            });
            if (yAxis != null) yAxis.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.Yaxis = value;
                UpdateVariablesForAll(VariableTypes.Y_AXIS, value);
            });

            if (zAxis != null) zAxis.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.Zaxis = value;
                UpdateVariablesForAll(VariableTypes.Z_AXIS, value);
            });

            if (dirDropDown != null) dirDropDown.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.direction = (Direction)value;
                UpdateVariablesForAll(VariableTypes.DIRECTION_DROPDOWN, value);
            });
            if (degreesInput != null) degreesInput.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.degrees =  Helper.StringToFloat(value);
                UpdateVariablesForAll(VariableTypes.DEGREES,  Helper.StringToFloat(value));
            });
            if (speedInput != null) speedInput.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.speed = Helper.StringToFloat(value);
                UpdateVariablesForAll(VariableTypes.SPEED,  Helper.StringToFloat(value));
            });
            if (pauseInput != null) pauseInput.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.pauseBetween = Helper.StringToFloat(value);
                UpdateVariablesForAll(VariableTypes.PAUSE,  Helper.StringToFloat(value));
            });
            if (repeatInput != null) repeatInput.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.repeat = Helper.StringInInt(value);
                UpdateVariablesForAll(VariableTypes.REPEAT,  Helper.StringInInt(value));
            });
            if (broadcastType != null) broadcastType.onValueChanged.AddListener((value) =>
            {
                string selectedString = EditorOp.Resolve<DataProvider>().GetListenString(value);
                if (selectedString.ToLower().Contains("custom"))
                    customString.transform.parent.gameObject.SetActive(true);
                else
                    customString.transform.parent.gameObject.SetActive(false);
                
                field.GetAtom().data.broadcastTypeIndex = value;
                field.GetAtom().data.broadcastName = broadcastType.options[value].text;
                ResetCustomString(broadcastType.options[value].text);
                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(selectedString, field.GetAtom().data.broadcastName, new ComponentDisplayDock() { componentGameObject = ((Atom.Rotate)field.Value).target, componentType = typeof(Atom.Rotate).Name });
                // UpdateVariablesForAll(VariableTypes.BROADCAST_STRING,  value);
            });
            if(customString != null) customString.onValueChanged.AddListener((value) =>
            {
                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(value, field.GetAtom().data.broadcastName, new ComponentDisplayDock() { componentGameObject = ((Atom.Rotate)field.Value).target, componentType = typeof(Atom.Rotate).Name });
                SetCustomString(value);
                // UpdateAllSelectedObjects("broadcast", field.GetAtom().data.broadcast);
            });
                
            if (broadcastAt != null) broadcastAt.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.broadcastAt = (BroadcastAt)value;
                UpdateVariablesForAll(VariableTypes.BROADCAST_AT, (BroadcastAt)value);
            });
            if (canListenMultipleTimesToggle != null) canListenMultipleTimesToggle.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.listen = value ? Listen.Always : Listen.Once;
                UpdateVariablesForAll(VariableTypes.CAN_LISTEN_MULTIPLE_TIMES,  field.GetAtom().data.listen);
            });
        }
        
        private void ShowCustomStringInput(int _index)
        {
            string selectedString = "None";
            if (_index < broadcastType.options.Count)
                selectedString = broadcastType.options[_index].text;
            // string selectedString = EditorOp.Resolve<DataProvider>().GetListenString(_index);
            Debug.Log("selected string "+selectedString);
            if (selectedString.ToLower().Contains("custom"))
                customString.transform.parent.gameObject.SetActive(true);
            else
                customString.transform.parent.gameObject.SetActive(false);
        }

        
        private void SetCustomString(string _newString)
        {
            Atom.Rotate atom = field.GetAtom();
            atom.data.broadcastName = _newString;
            EditorOp.Resolve<DataProvider>().UpdateToListenList(atom.id, _newString);
        }
        
        private void ResetCustomString(string _broadcastType)
        {
            if (_broadcastType.ToLower().Contains("custom"))
            {
                field.GetAtom().data.broadcastName = "";
                customString.SetTextWithoutNotify("");
            }
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
                        Rotate comp = obj.GetComponent<Rotate>();
                        switch (_type)
                        {
                            case VariableTypes.X_AXIS:
                                // Debug.Log("setting for x "+obj.name);
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
                                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(
                                    _value.ToString(), comp.Type.data.broadcastName, new ComponentDisplayDock() { componentGameObject = obj, componentType = typeof(Atom.Rotate).Name });
                                comp.Type.data.broadcastName = (string)_value;
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

            if (broadcastType != null)
            {
                broadcastType.options.Clear();
                List<string> newList = EditorOp.Resolve<DataProvider>().ListenToTypes;
                foreach (string _name in newList)
                {
                    broadcastType.options.Add(new Dropdown.OptionData()
                    {
                        text = _name
                    });
                }
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
            if (broadcastType) broadcastType.SetValueWithoutNotify(_data.broadcastTypeIndex);
            if (customString) customString.SetTextWithoutNotify( _data.broadcastName);
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
            broadcastType?.SetSkinDropDownField(Skin);
            listenInput?.SetupInputFieldSkin(Skin);
            dirDropDown?.SetSkinDropDownField(Skin);
            broadcastAt?.SetSkinDropDownField(Skin);
            broadcastType?.SetSkinDropDownField(Skin);
            customString?.SetupInputFieldSkin(Skin);
        }
    }
}