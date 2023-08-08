using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Terra.Studio;
using PlayShifu.Terra;

namespace RuntimeInspectorNamespace
{
    public class TranslateTypes : MonoBehaviour
    {
        public TMP_InputField speedInput = null;
        public TMP_InputField pauseDistanceInput = null;
        public TMP_InputField pauseForInput = null;
        public TMP_InputField repeatInput = null;

        public Dropdown broadcastAt;
        public TMP_InputField broadcastInput;

        [HideInInspector]
        public TranslateField translateField = null;
        private TranslateComponentData data = new TranslateComponentData();
        
        public void Setup()
        {
            LoadDefaultValues();
            if (pauseDistanceInput != null) pauseDistanceInput.onValueChanged.AddListener(
                (value) => { ;
                    data.pauseAtDistance = Helper.StringToFloat(value);
                    translateField.UpdateData(data);
                });
            if (speedInput != null) speedInput.onValueChanged.AddListener((value) =>
            {
                data.speed = Helper.StringToFloat(value);
                translateField.UpdateData(data);
            });
            if (pauseForInput != null) pauseForInput.onValueChanged.AddListener((value) =>
            {
                data.pauseFor = Helper.StringToFloat(value);
                translateField.UpdateData(data);
            });
            if (repeatInput != null) repeatInput.onValueChanged.AddListener((value) =>
            {
                data.repeat = Helper.StringInInt(value);
                translateField.UpdateData(data);
            });
            if(broadcastInput != null) broadcastInput.onValueChanged.AddListener((value) =>
            {
                data.broadcast = value;
            });
            if (broadcastAt != null) broadcastAt.onValueChanged.AddListener((value) =>
            {
                data.broadcastAt = getBroadcastAt(broadcastAt.options[value].text);
                translateField.UpdateData(data);
            });

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

        private BroadcastAt getBroadcastAt(string _value)
        {
            if (_value == BroadcastAt.End.ToString()) return BroadcastAt.End;
            else if (_value == BroadcastAt.Never.ToString()) return BroadcastAt.Never;
            else if (_value == BroadcastAt.AtEveryInterval.ToString()) return BroadcastAt.AtEveryInterval;
            return BroadcastAt.Never;
        }
        
        public void LoadDefaultValues()
        {
            // broadcast at 
            if (broadcastAt != null){broadcastAt.AddOptions(Enum.GetNames(typeof(BroadcastAt)).ToList());}
        }

        public TranslateComponentData GetData()
        {
            return data;
        }

        public void SetData(TranslateComponentData _data)
        {
            if(broadcastAt != null) broadcastAt.value = ((int)Enum.Parse(typeof(BroadcastAt), _data.broadcastAt.ToString()));

            if (pauseDistanceInput != null) pauseDistanceInput.text = _data.pauseAtDistance.ToString();
            if (speedInput != null) speedInput.text = _data.speed.ToString();
            if (pauseForInput != null) pauseForInput.text = _data.pauseFor.ToString();
            if (pauseDistanceInput != null) pauseDistanceInput.text = _data.pauseAtDistance.ToString();
            if (repeatInput != null) repeatInput.text = _data.repeat.ToString();
            if (broadcastInput != null) broadcastInput.text = _data.broadcast;
            
        }
    }
}