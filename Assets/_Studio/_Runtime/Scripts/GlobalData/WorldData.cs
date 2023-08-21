using System;
using UnityEngine;
using Newtonsoft.Json;

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
        public AssetType assetType;
        public PrimitiveType primitiveType;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 position;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 rotation;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 scale;
        public EntityBasedComponent[] components;
        public VirtualEntity[] children;
    }

    [Serializable]
    public struct EntityBasedComponent
    {
        public string type;
        public object data;
    }

    public enum AssetType
    {
        Empty,
        Primitive,
        Prefab
    }
}