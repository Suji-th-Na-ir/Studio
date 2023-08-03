using UnityEngine;
using Terra.Studio;

namespace RuntimeInspectorNamespace
{
    public class RTypeIncremental : MonoBehaviour
    {
        public Axis Axis;
        public Direction Direction;
        public float Degrees = 0;
        public float Speed = 0;
        public int Increment = 0;
        public float PauseBetweenIncrements = 0;
        public int Repeat = 0;
    }
}
