using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Terra.Studio;

namespace RuntimeInspectorNamespace
{
    public class RTypeIncremental : MonoBehaviour
    {
        public GlobalEnums.Axis Axis;
        public GlobalEnums.Direction Direction;
        public float Degrees = 0;
        public float Speed = 0;
        public int Increment = 0;
        public float PauseBetweenIncrements = 0;
        public int Repeat = 0;
    }
}
