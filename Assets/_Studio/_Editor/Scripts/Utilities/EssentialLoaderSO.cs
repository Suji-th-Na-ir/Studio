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

        [SerializeField]
        private List<EssentialsData> data;
        public IEnumerable<EssentialsData> Essentials { get { return data; } }

        [ContextMenu("Paste")]
        private void PasteCopiedValue()
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
            data.Add(newData);
        }
    }
}
