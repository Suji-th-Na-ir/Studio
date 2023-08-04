using System;
using Newtonsoft.Json;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;


namespace RuntimeInspectorNamespace
{
    [Serializable]
    public class RotateComponentData
    {
        public RotationType rotateType;
        public string axis = null;
        public string direction = null;
        public float degrees = 0f;
        public float speed = 0f;
        public int repeat = 0;
        public float increment = 0f;
        public float pauseBetween = 0f;
    }
    public class Rotate : MonoBehaviour, IComponent
    {
        public StartOn Start = StartOn.GameStart;
        public Atom.Rotate rfield = new Atom.Rotate();

        private RotateComponent rComp;


        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.H)) Export();
        }

        public (string type, string data) Export()
        {
            RotateComponent rc = new();
            rc.IsBroadcastable = true;
            
            rc.axis =  GetAxis(rfield.rData.axis);
            rc.direction = GetDirection(rfield.rData.direction);
            rc.rotationType = rfield.rData.rotateType;
            rc.repeatType = GetRepeatType(rfield.rData.repeat);
            rc.speed = rfield.rData.speed;
            rc.rotateBy = rfield.rData.degrees;
            rc.pauseFor = rfield.rData.pauseBetween;
            rc.repeatFor = rfield.rData.repeat;
            
            
            string type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(rc, Formatting.Indented);
            
            Debug.Log(data);
            
            return (type, data);
        }

        private RepeatType GetRepeatType(float _value)
        {
            if (_value == 0) return RepeatType.Forever;
            else return RepeatType.XTimes;
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

        public void Import(EntityBasedComponent data)
        {
            
        }
    }
}
