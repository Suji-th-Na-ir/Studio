using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Terra.Studio
{
    [CreateAssetMenu(menuName = "Terra/EssentialsLoader", fileName = "EssentialsLoaderSO")]
    public class EssentialLoaderSO : ScriptableObject
    {
        [Serializable]
        public struct EssentialsData
        {
            public ResourceDB.ResourceItemData itemData;
            public EditorObjectType type;
            public Vector3 spawnPosition;
        }

        [SerializeField] private List<EssentialsData> essentials;
        [SerializeField] private List<EssentialsData> loadOnDemandData;
        public IEnumerable<EssentialsData> Essentials { get { return essentials; } }

        public bool TryGetData(EditorObjectType type, out EssentialsData objData)
        {
            objData = loadOnDemandData.Find(x => x.type == type);
            var isDataAvailable = !objData.Equals(default(EssentialsData));
            return isDataAvailable;
        }

        [ContextMenu("Paste to essentials")]
        private void PasteCopiedValue_ToEssentials()
        {
            var json = GUIUtility.systemCopyBuffer;
            ResourceDB.ResourceItemData itemData = null;
            try
            {
                itemData = JsonConvert.DeserializeObject<ResourceDB.ResourceItemData>(json);
            }
            catch
            {
                Debug.Log($"{json} is not a valid copied content!");
            }
            if (itemData == null)
            {
                return;
            }
            var newData = new EssentialsData()
            {
                itemData = itemData
            };
            essentials.Add(newData);
        }

        [ContextMenu("Paste to load on demand")]
        private void PasteCopiedValue_ToLoadOnDemand()
        {
            var json = GUIUtility.systemCopyBuffer;
            ResourceDB.ResourceItemData itemData = null;
            try
            {
                itemData = JsonConvert.DeserializeObject<ResourceDB.ResourceItemData>(json);
            }
            catch
            {
                Debug.Log($"{json} is not a valid copied content!");
            }
            if (itemData == null)
            {
                return;
            }
            var newData = new EssentialsData()
            {
                itemData = itemData
            };
            loadOnDemandData.Add(newData);
        }
    }
}
