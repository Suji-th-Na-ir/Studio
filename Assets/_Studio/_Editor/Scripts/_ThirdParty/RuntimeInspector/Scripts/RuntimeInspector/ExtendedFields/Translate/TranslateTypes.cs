using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Terra.Studio;
using UnityEngine.UI;
using PlayShifu.Terra;

namespace RuntimeInspectorNamespace
{
    public class TranslateTypes : MonoBehaviour
    {
        public InputField[] moveToInput;
        public TMP_InputField speedInput = null;
        public TMP_InputField repeatInput = null;
        public TMP_InputField pauseForInput = null;
        public TMP_InputField listenTo = null;

        public Dropdown broadcastAt;
        public TMP_InputField broadcastInput;

        [HideInInspector]
        public TranslateField translateField = null;
        private TranslateComponentData data = new TranslateComponentData();

        public void Setup()
        {
            LoadDefaultValues();
            if (moveToInput != null) moveToInput[0].onValueChanged.AddListener(
                (value) =>
                {
                    data.moveTo.x = Helper.StringToFloat(value);
                    UpdateData(data);
                });
            
            if (moveToInput != null) moveToInput[1].onValueChanged.AddListener(
                (value) =>
                {
                    data.moveTo.y = Helper.StringToFloat(value);
                    UpdateData(data);
                });
            
            if (moveToInput != null) moveToInput[2].onValueChanged.AddListener(
                (value) =>
                {
                    data.moveTo.z = Helper.StringToFloat(value);
                    UpdateData(data);
                });
            if (speedInput != null) speedInput.onValueChanged.AddListener((value) =>
            {
                data.speed = Helper.StringToFloat(value);
                UpdateData(data);
            });
            if (pauseForInput != null) pauseForInput.onValueChanged.AddListener((value) =>
            {
                data.pauseFor = Helper.StringToFloat(value);
                UpdateData(data);
            });
            if (listenTo != null) listenTo.onValueChanged.AddListener((value) =>
            {
                data.listenTo = value;
                UpdateData(data);
            });
            if (repeatInput != null) repeatInput.onValueChanged.AddListener((value) =>
            {
                data.repeat = Helper.StringInInt(value);
                UpdateData(data);
            });
            if (broadcastInput != null) broadcastInput.onValueChanged.AddListener((value) =>
            {
                data.broadcast = value;
                UpdateData(data);
            });
            if (broadcastAt != null) broadcastAt.onValueChanged.AddListener((value) =>
            {
                data.broadcastAt = GetBroadcastAt(broadcastAt.options[value].text);
                UpdateData(data);
            });
        }
        
        private void UpdateData(TranslateComponentData _data)
        {
            translateField.UpdateData(_data);
            UpdateOtherSelectedObjects(_data);
        }

        private void UpdateOtherSelectedObjects(TranslateComponentData _data)
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();

            if (selectedObjects.Count > 1)
            {
                foreach (var obj in selectedObjects)
                {
                    if (obj.GetComponent<Translate>() != null)
                    {
                        Translate translate = obj.GetComponent<Translate>();
                        translate.Type.data = Helper.DeepCopy(_data);
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
            // broadcast at 
            if (broadcastAt != null) { broadcastAt.AddOptions(Enum.GetNames(typeof(BroadcastAt)).ToList()); }
        }
        
        public void ResetValues()
        {
            TranslateComponentData td = translateField.GetAtomTranslateData();
            td.moveTo = Vector3.zero;
            td.pauseFor = 0f;
            td.speed = 0f;
            td.repeat = 0;
            td.broadcast = "";
            td.broadcastAt = BroadcastAt.Never;
            td.listenTo = "";
            td.targetPosition = Vector3.zero;
        }

        public TranslateComponentData GetData()
        {
            return data;
        }

        public void SetData(TranslateComponentData _data)
        {
            data = _data;
            if (broadcastAt != null) broadcastAt.value = ((int)Enum.Parse(typeof(BroadcastAt), _data.broadcastAt.ToString()));
            if (pauseForInput != null) pauseForInput.text = _data.pauseFor.ToString();
            if (speedInput != null) speedInput.text = _data.speed.ToString();
            if (repeatInput != null) repeatInput.text = _data.repeat.ToString();
            if (broadcastInput != null) broadcastInput.text = _data.broadcast;
            if (moveToInput != null) moveToInput[0].text = _data.moveTo.x.ToString();
            if (moveToInput != null) moveToInput[1].text = _data.moveTo.y.ToString();
            if (moveToInput != null) moveToInput[2].text = _data.moveTo.z.ToString();
            if (listenTo != null) listenTo.text = _data.listenTo;
        }
    }
}