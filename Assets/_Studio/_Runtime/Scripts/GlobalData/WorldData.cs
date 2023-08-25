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
        public MetaData metaData;
    }

    [Serializable]
    public struct EntityBasedComponent
    {
        public string type;
        public object data;
    }

    [Serializable]
    public struct MetaData
    {
        public ColliderData colliderData;
    }

    [Serializable]
    public struct ColliderData
    {
        public bool doesHaveCollider;
        public bool isTrigger;
        public string type;
        public float radius;
        public float height;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 size;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 center;
    }

    public enum AssetType
    {
        Empty,
        Primitive,
        Prefab
    }
}