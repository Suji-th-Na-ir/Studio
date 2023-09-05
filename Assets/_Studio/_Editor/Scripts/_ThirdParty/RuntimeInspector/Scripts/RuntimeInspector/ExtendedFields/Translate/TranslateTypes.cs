using TMPro;
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
        public InputField[] moveToInput;
        public InputField speedInput = null;
        public InputField repeatInput = null;
        public InputField pauseForInput = null;
        public InputField listenTo = null;

        public Dropdown broadcastAt;
        
        public Dropdown broadcastType;
        public InputField customString;
        
        public Toggle canListenMultipleTimesToggle;

        [HideInInspector] public TranslateField field = null;
        private string guid;

        private void Awake()
        {
            guid = GetInstanceID() + "_translate";//Guid.NewGuid().ToString("N");
        }
        
        public void Setup()
        {
            LoadDefaultValues();
            moveToInput?[0].onValueChanged.AddListener(
                (value) =>
                {
                    field.GetAtom().data.moveTo.x = Helper.StringToFloat(value);
                    UpdateAllSelectedObjects("moveTo", field.GetAtom().data.moveTo);
                });

            moveToInput?[1].onValueChanged.AddListener(
                (value) =>
                {
                    field.GetAtom().data.moveTo.y = Helper.StringToFloat(value);
                    UpdateAllSelectedObjects("moveTo", field.GetAtom().data.moveTo);
                });

            moveToInput?[2].onValueChanged.AddListener(
                (value) =>
                {
                    field.GetAtom().data.moveTo.z = Helper.StringToFloat(value);
                    UpdateAllSelectedObjects("moveTo", field.GetAtom().data.moveTo);
                });
            if (speedInput != null) speedInput.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.speed = Helper.StringToFloat(value);
                UpdateAllSelectedObjects("speed", field.GetAtom().data.speed);
            });
            if (pauseForInput != null) pauseForInput.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.pauseFor = Helper.StringToFloat(value);
                UpdateAllSelectedObjects("pauseFor", field.GetAtom().data.pauseFor);
            });
            if (repeatInput != null) repeatInput.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.repeat = Helper.StringInInt(value);
                UpdateAllSelectedObjects("repeat", field.GetAtom().data.repeat);
            });
            if (broadcastType != null) broadcastType.onValueChanged.AddListener((value) =>
            {
                string selectedString = EditorOp.Resolve<DataProvider>().GetListenString(value);
                if (selectedString.ToLower().Contains("custom"))
                    customString.transform.parent.gameObject.SetActive(true);
                else
                    customString.transform.parent.gameObject.SetActive(false);
                
                field.GetAtom().data.broadcastTypeIndex = value;
                ResetCustomString();
                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(selectedString, field.GetAtom().data.broadcastName, new ComponentDisplayDock() { componentGameObject = ((Atom.Translate)field.Value).target, componentType = typeof(Atom.Translate).Name });
                // UpdateVariablesForAll(VariableTypes.BROADCAST_STRING,  value);
            });
            if (broadcastAt != null) broadcastAt.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.broadcastAt = GetBroadcastAt(broadcastAt.options[value].text);
                UpdateAllSelectedObjects("broadcastAt", field.GetAtom().data.broadcastAt);
            });
            if (canListenMultipleTimesToggle != null) canListenMultipleTimesToggle.onValueChanged.AddListener((value) =>
            {
                field.GetAtom().data.listen = value ? Listen.Always : Listen.Once;
                UpdateAllSelectedObjects("listen", field.GetAtom().data.listen);
            });
        }
        
        private void ResetCustomString()
        {
            field.GetAtom().data.broadcastName = "";
            customString.text = "";
        }


        private void UpdateAllSelectedObjects(string varName, object value)
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            foreach (var obj in selectedObjects)
            {
                if (obj.TryGetComponent(out Translate translate))
                {
                    var type = typeof(Translate);
                    var typeField = type.GetField("Type", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
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
            if (broadcastAt != null) { broadcastAt.AddOptions(Enum.GetNames(typeof(BroadcastAt)).ToList()); }
        }

        public void SetData(TranslateComponentData _data)
        {
            field.GetAtom().data = _data;
            if (broadcastAt != null) broadcastAt.SetValueWithoutNotify((int)Enum.Parse(typeof(BroadcastAt), _data.broadcastAt.ToString()));
            if (pauseForInput != null) pauseForInput.SetTextWithoutNotify(_data.pauseFor.ToString());
            if (speedInput != null) speedInput.SetTextWithoutNotify(_data.speed.ToString());
            if (repeatInput != null) repeatInput.SetTextWithoutNotify(_data.repeat.ToString());
            // if (broadcastInput != null) broadcastInput.SetTextWithoutNotify(_data.broadcast);
            if (moveToInput != null) moveToInput[0].SetTextWithoutNotify(_data.moveTo.x.ToString());
            if (moveToInput != null) moveToInput[1].SetTextWithoutNotify(_data.moveTo.y.ToString());
            if (moveToInput != null) moveToInput[2].SetTextWithoutNotify(_data.moveTo.z.ToString());
            if (broadcastType) broadcastType.SetValueWithoutNotify(_data.broadcastTypeIndex);
            if (canListenMultipleTimesToggle) canListenMultipleTimesToggle.SetIsOnWithoutNotify(_data.listen == Listen.Always);
        }

        public void ApplySkin(UISkin skin)
        {
            canListenMultipleTimesToggle?.SetupToggeleSkin(skin);
            broadcastAt?.SetSkinDropDownField(skin);
            for (int i = 0; i < moveToInput.Length; i++)
            {
                moveToInput[i]?.SetupInputFieldSkin(skin);
            }
            speedInput?.SetupInputFieldSkin(skin);
            pauseForInput?.SetupInputFieldSkin(skin);
            repeatInput?.SetupInputFieldSkin(skin);
            listenTo?.SetupInputFieldSkin(skin);
            broadcastType?.SetSkinDropDownField(skin);
            customString?.SetupInputFieldSkin(skin);
        }
    }
}