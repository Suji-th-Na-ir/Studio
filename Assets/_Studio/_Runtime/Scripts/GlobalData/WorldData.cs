using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
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

        public string name;
        
        public string assetPath;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 position;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 rotation;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 scale;

        public EntityBasedComponent[] components;
        public Dictionary<int, EntityBasedComponent[]> childCompenentDictionary;
    }

    [Serializable]
    public struct EntityBasedComponent
    {
        public string type;
        public object data;
    }
}