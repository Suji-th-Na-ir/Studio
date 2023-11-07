using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class LoadedPoolValidator : IDisposable
    {
        private GameObject rootGo;
        private Dictionary<string, GameObject> uniqueLoadedMap;
        public LoadedPoolValidator()
        {
            rootGo = new GameObject();
            rootGo.gameObject.name = "LoadedPoolValidation";
            uniqueLoadedMap = new();
            Object.DontDestroyOnLoad(rootGo);
        }

        public void AddToPool(string uniqueName, GameObject go)
        {
            if (!uniqueLoadedMap.ContainsKey(uniqueName))
            {
                var temp = Object.Instantiate(go, rootGo.transform, true);
                if (temp.TryGetComponent(out StudioGameObject sgo))
                {
                    Object.Destroy(sgo);
                }
                temp.SetActive(false);
                uniqueLoadedMap.Add(uniqueName, temp);
            }
            else
            {
                return;
            }
        }
        public bool IsInLoadedPool(string uniqueName)
        {
            return uniqueLoadedMap.ContainsKey(uniqueName);
        }

        public void RemoveFromPool(string uniqueName)
        {
            if (uniqueLoadedMap.ContainsKey(uniqueName))
            {
                var prefab = uniqueLoadedMap[uniqueName];
                if (prefab != null)
                {
                    Object.Destroy(prefab);
                }
                var x = uniqueLoadedMap.Remove(uniqueName);
            }
        }

        public GameObject GetDuplicateFromPool(string uniqueName)
        {
            // return null;
            if (uniqueLoadedMap.TryGetValue(uniqueName, out var prefab))
            {
                var temp = Object.Instantiate(prefab);
                temp.name = temp.name.Replace("(Clone)", "");
                temp.SetActive(true);
                return temp;
            }

            return null;
        }

        public void Dispose()
        {
            foreach (var (key, prefab) in uniqueLoadedMap)
            {
                if (prefab != null)
                {
                    Object.Destroy(prefab);
                }

                uniqueLoadedMap[key] = null;
            }

            uniqueLoadedMap = null;
        }
    }
}