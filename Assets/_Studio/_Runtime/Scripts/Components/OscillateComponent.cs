using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [Serializable]
    public struct OscillateComponent : IBaseComponent, IConditional
    {
        public float speed;
        [JsonProperty("fromPoint")]
        public float[] startPoint;
        [JsonProperty("toPoint")]
        public float[] endPoint;
        [JsonIgnore]
        public Vector3 fromPoint
        {
            get
            {
                if (startPoint == null)
                {
                    return Vector3.zero;
                }
                return new Vector3(startPoint[0], startPoint[1], startPoint[2]);
            }
            set
            {
                if (startPoint == null)
                {
                    startPoint = new float[3];
                }
                startPoint[0] = value.x;
                startPoint[1] = value.y;
                startPoint[2] = value.z;
            }
        }
        [JsonIgnore]
        public Vector3 toPoint
        {
            get
            {
                if (endPoint == null)
                {
                    return Vector3.zero;
                }
                return new Vector3(endPoint[0], endPoint[1], endPoint[2]);
            }
            set
            {
                if (endPoint == null)
                {
                    endPoint = new float[3];
                }
                endPoint[0] = value.x;
                endPoint[1] = value.y;
                endPoint[2] = value.z;
            }
        }
        public bool loop;
        public bool CanExecute { get; set; }
        public string ConditionType { get; set; }
        public string ConditionData { get; set; }
        public bool IsConditionAvailable { get; set; }
        [JsonIgnore] public bool isRegistered;
        [JsonIgnore] public Transform oscillatableTr;
        [JsonIgnore] public bool IsExecuted { get; set; }
    }
}
