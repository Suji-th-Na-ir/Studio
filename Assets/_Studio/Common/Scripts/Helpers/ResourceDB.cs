using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Terra.Studio
{
    [CreateAssetMenu(fileName = "ResourceDB", menuName = "Terra/ResourceDB")]
    public class ResourceDB : ScriptableObject
    {
        [Serializable]
        public class ResourceItemData
        {
            [SerializeField] private string name;
            [SerializeField] private string absolutePath;
            [SerializeField] private string resourcePath;
            [SerializeField] private string type;
            [SerializeField] private string guid;
            [SerializeField] private bool isPrimitive;
            [SerializeField] private PrimitiveType primitiveType;

            public string AbsolutePath { get { return absolutePath; } }
            public string ResourcePath { get { return resourcePath; } }
            public string Name { get { return name; } }
            public string Type { get { return type; } }
            public string Guid { get { return guid; } }
            public bool IsPrimitive { get { return isPrimitive; } }
            public PrimitiveType PrimitiveType { get { return primitiveType; } }

            public ResourceItemData(string name, string absolutePath, string resourcePath, string type, string guid, bool isPrimitive = false, PrimitiveType primitiveType = default)
            {
                this.absolutePath = absolutePath;
                this.resourcePath = resourcePath;
                this.name = name;
                this.type = type;
                this.guid = guid;
                this.isPrimitive = isPrimitive;
                this.primitiveType = primitiveType;
            }
        }

        [HideInInspector, SerializeField]
        private List<ResourceItemData> itemsData = new();

        public ResourceItemData GetItemDataForNearestName(string name)
        {
            var data = itemsData.Find(x => x.Name.Equals(name));
            return data;
        }

        public ResourceItemData GetItemDataForPath(string assetPath)
        {
            var data = itemsData.Find(x => x.ResourcePath.Equals(assetPath));
            return data;
        }

        public T[] GetAllTypes<T>(string path) where T : UnityEngine.Object
        {
            var typeName = typeof(T).AssemblyQualifiedName;
            var filters = itemsData.
                Where(x => x.Type.Equals(typeName) && x.ResourcePath.Contains(path)).
                Select(y => y.ResourcePath);
            var loadedAssets = new List<T>();
            foreach (var filter in filters)
            {
                var asset = SystemOp.Load<T>(filter);
                if (asset)
                {
                    loadedAssets.Add(asset);
                }
            }
            return loadedAssets.ToArray();
        }

        public static T[] GetAll<T>(string path) where T : UnityEngine.Object
        {
            var instance = (ResourceDB)SystemOp.Load(ResourceTag.ResourceDB);
            return instance.GetAllTypes<T>(path);
        }

        public static ResourceItemData GetItemData(string assetPath)
        {
            var instance = (ResourceDB)SystemOp.Load(ResourceTag.ResourceDB);
            return instance.GetItemDataForPath(assetPath);
        }

        public static ResourceItemData GetDummyItemData(PrimitiveType type)
        {
            var itemData = new ResourceItemData(type.ToString(), null, null, typeof(GameObject).AssemblyQualifiedName, Guid.NewGuid().ToString("N"), true, type);
            return itemData;
        }

#if UNITY_EDITOR
        [HideInInspector]
        public List<string> watchFolders = new();
        public List<ResourceItemData> ItemsData { get { return itemsData; } }
        public static ResourceDB db;

        public static string GetAssetResourcePath(string absolutePath)
        {
            if (!db)
            {
                db = Resources.Load<ResourceDB>("System/ResourceDB");
            }
            var itemData = db.ItemsData;
            if (itemData.Any(x => x.AbsolutePath.Equals(absolutePath)))
            {
                var data = itemData.Find(x => x.AbsolutePath.Equals(absolutePath));
                return data.ResourcePath;
            }
            return null;
        }
#endif
    }
}