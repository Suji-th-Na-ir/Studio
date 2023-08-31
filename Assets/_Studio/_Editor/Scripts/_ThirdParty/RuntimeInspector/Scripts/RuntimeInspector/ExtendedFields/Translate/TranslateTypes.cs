using TMPro;
using System;
using System.Linq;
using UnityEngine;
using Terra.Studio;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Serialization;

namespace RuntimeInspectorNamespace
{
    public class TranslateTypes : MonoBehaviour
    {
        public InputField[] moveToInput;
        public TMP_InputField speedInput = null;
        public TMP_InputField repeatInput = null;
        public TMP_InputField pauseForInput = null;

        public Dropdown broadcastAt;
        public TMP_InputField broadcastInput = null;
        public Toggle canListenMultipleTimesToggle;

        [HideInInspector] public TranslateField field = null;
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
            MOVE_TO_X,
            MOVE_TO_Y,
            MOVE_TO_Z,
            SPEED,
            PAUSE_FOR,
            REPEAT,
            BROADCAST_AT,
            BROADCAST_STRING,
            LISTEN
        }
        
        private void UpdateVariablesForAll(VariableTypes _type, System.Object _value)
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selectedObjects.Count > 1)
            {
                foreach (var obj in selectedObjects)
                {
                    if (obj.GetComponent<Translate>() != null)
                    {
                        Translate comp = obj.GetComponent<Translate>();
                        switch (_type)
                        {
                            case VariableTypes.MOVE_TO_X:
                                Debug.Log("updating x for "+obj.name);
                                comp.Type.data.moveTo.x = (float)_value;
                                break;
                            case VariableTypes.MOVE_TO_Y:
                                comp.Type.data.moveTo.y = (float)_value;
                                break;
                            case VariableTypes.MOVE_TO_Z:
                                comp.Type.data.moveTo.z = (float)_value;
                                break;
                            case VariableTypes.SPEED:
                                comp.Type.data.speed = (float)_value;
                                break;
                            case VariableTypes.PAUSE_FOR:
                                comp.Type.data.pauseFor = (float)_value;
                                break;
                            case VariableTypes.REPEAT:
                                comp.Type.data.repeat = (int)_value;
                                break;
                            case VariableTypes.BROADCAST_AT:
                                comp.Type.data.broadcastAt = (BroadcastAt)_value;
                                break;
                            case VariableTypes.BROADCAST_STRING:
                                comp.Type.data.broadcast = (string)_value;
                                break;
                            case VariableTypes.LISTEN:
                                comp.Type.data.listen = (Listen)_value;
                                break;
                        }
                    }
                }
            }
        }
        
        public void Setup()
        {
            LoadDefaultValues();
            if (moveToInput != null) moveToInput[0].onValueChanged.AddListener(
                (value) =>
                {
                    field.GetAtom().data.moveTo.x = Helper.StringToFloat(value);
                    UpdateVariablesForAll(VariableTypes.MOVE_TO_X,  Helper.StringToFloat(value));
                });

            if (moveToInput != null) moveToInput[1].onValueChanged.AddListener(
                (value) =>
                {
                    field.GetAtom().data.moveTo.y = Helper.StringToFloat(value);
                    UpdateVariablesForAll(VariableTypes.MOVE_TO_Y,  Helper.StringToFloat(value));
                });

            if (moveToInput != null) moveToInput[2].onValueChanged.AddListener(
                (value) =>
                {
                    field.GetAtom().data.moveTo.z = Helper.StringToFloat(value);
                    UpdateVariablesForAll(VariableTypes.MOVE_TO_Z,  Helper.StringToFloat(value));
                });
            if (speedInput != null) speedInput.onValueChanged.AddListener(
                (value) =>
            {
                field.GetAtom().data.speed = Helper.StringToFloat(value);
                UpdateVariablesForAll(VariableTypes.SPEED,  Helper.StringToFloat(value));
            });
            if (pauseForInput != null) pauseForInput.onValueChanged.AddListener(
                (value) =>
                {
                    field.GetAtom().data.pauseFor = Helper.StringToFloat(value);
                    UpdateVariablesForAll(VariableTypes.PAUSE_FOR,  Helper.StringToFloat(value));
                });
            if (repeatInput != null) repeatInput.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.repeat = Helper.StringInInt(value);
                UpdateVariablesForAll(VariableTypes.REPEAT,  Helper.StringInInt(value));
            });
            if (broadcastInput != null) broadcastInput.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.broadcast = value;
                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(value, 
                    value, new ComponentDisplayDock() { componentGameObject = ((Atom.Translate)field.Value).referenceGO, componentType = typeof(Atom.Translate).Name });
                UpdateVariablesForAll(VariableTypes.BROADCAST_STRING, value);
            });
            if (broadcastAt != null) broadcastAt.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.broadcastAt = (BroadcastAt)value;
                UpdateVariablesForAll(VariableTypes.BROADCAST_AT, (BroadcastAt)value);
            });
            if (canListenMultipleTimesToggle != null) canListenMultipleTimesToggle.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.listen = value ? Listen.Always : Listen.Once;
                UpdateVariablesForAll(VariableTypes.LISTEN, field.GetAtom().data.listen);
            });
        }
        
        public void LoadDefaultValues()
        {
            if (broadcastAt != null) { broadcastAt.AddOptions(Enum.GetNames(typeof(BroadcastAt)).ToList()); }
        }

        public void SetData(TranslateComponentData _data)
        {
            if (broadcastAt != null) broadcastAt.value = ((int)Enum.Parse(typeof(BroadcastAt), _data.broadcastAt.ToString()));
            if (pauseForInput != null) pauseForInput.text = _data.pauseFor.ToString();
            if (speedInput != null) speedInput.text = _data.speed.ToString();
            if (repeatInput != null) repeatInput.text = _data.repeat.ToString();
            if (broadcastInput != null) broadcastInput.text = _data.broadcast;
            if (moveToInput != null) moveToInput[0].text = _data.moveTo.x.ToString();
            if (moveToInput != null) moveToInput[1].text = _data.moveTo.y.ToString();
            if (moveToInput != null) moveToInput[2].text = _data.moveTo.z.ToString();
            if (canListenMultipleTimesToggle) canListenMultipleTimesToggle.SetIsOnWithoutNotify(_data.listen == Listen.Always);
        }
    }
}