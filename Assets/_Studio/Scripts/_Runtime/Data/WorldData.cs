using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Terra.Studio
{
    [Serializable]
    public struct WorldData
    {
        public VirtualEntity[] entities;
    }

    [Serializable]
    public struct VirtualEntity
    {
        public int id;
        public string primitiveType;
        [JsonProperty("position")]
        public float[] positionArray;
        [JsonProperty("rotation")]
        public float[] rotationArray;
        [JsonProperty("scale")]
        public float[] scaleArray;
        [JsonIgnore]
        public Vector3 position
        {
            get
            {
                if (positionArray == null)
                {
                    return Vector3.zero;
                }
                return new Vector3(positionArray[0], positionArray[1], positionArray[2]);
            }
            set
            {
                if (positionArray == null)
                {
                    positionArray = new float[3];
                }
                positionArray[0] = value.x;
                positionArray[1] = value.y;
                positionArray[2] = value.z;
            }
        }
        [JsonIgnore]
        public Vector3 rotation
        {
            get
            {
                if (rotationArray == null)
                {
                    return Vector3.zero;
                }
                return new Vector3(rotationArray[0], rotationArray[1], rotationArray[2]);
            }
            set
            {
                if (rotationArray == null)
                {
                    rotationArray = new float[3];
                }
                rotationArray[0] = value.x;
                rotationArray[1] = value.y;
                rotationArray[2] = value.z;
            }
        }
        [JsonIgnore]
        public Vector3 scale
        {
            get
            {
                if (scaleArray == null)
                {
                    return Vector3.zero;
                }
                return new Vector3(scaleArray[0], scaleArray[1], scaleArray[2]);
            }
            set
            {
                if (scaleArray == null)
                {
                    scaleArray = new float[3];
                }
                scaleArray[0] = value.x;
                scaleArray[1] = value.y;
                scaleArray[2] = value.z;
            }
        }
        public EntityBasedComponent[] components;
    }

    [Serializable]
    public struct EntityBasedComponent
    {
        public string type;
        public object data;
    }
}