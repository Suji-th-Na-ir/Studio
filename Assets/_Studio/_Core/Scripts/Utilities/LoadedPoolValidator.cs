using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class LoadedPoolValidator : IDisposable
    {
        private readonly GameObject _rootGo;
        private Dictionary<string, GameObject> _uniqueLoadedMap;
        public LoadedPoolValidator()
        {
            _rootGo = new GameObject
            {
                gameObject =
                {
                    name = "LoadedPoolValidation"
                }
            };
            _uniqueLoadedMap = new();
            Object.DontDestroyOnLoad(_rootGo);
        }

        public void AddToPool(string uniqueName, GameObject go)
        {
            if (!_uniqueLoadedMap.ContainsKey(uniqueName))
            {
                var temp = Object.Instantiate(go, _rootGo.transform, true);
                if (temp.TryGetComponent(out StudioGameObject sgo))
                {
                    Object.Destroy(sgo);
                }
                temp.SetActive(false);
                _uniqueLoadedMap.Add(uniqueName, temp);
            }
            else
            {
                return;
            }
        }
        public bool IsInLoadedPool(string uniqueName)
        {
            return _uniqueLoadedMap.ContainsKey(uniqueName);
        }

        public void RemoveFromPool(string uniqueName)
        {
            if (_uniqueLoadedMap.ContainsKey(uniqueName))
            {
                var prefab = _uniqueLoadedMap[uniqueName];
                if (prefab != null)
                {
                    Object.Destroy(prefab);
                }
                var x = _uniqueLoadedMap.Remove(uniqueName);
            }
        }

        public GameObject GetDuplicateFromPool(string uniqueName)
        {
            if (_uniqueLoadedMap.TryGetValue(uniqueName, out var prefab))
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
            foreach (var (key, prefab) in _uniqueLoadedMap)
            {
                if (prefab != null)
                {
                    Object.Destroy(prefab);
                }

                // _uniqueLoadedMap[key] = null;
            }
            _uniqueLoadedMap.Clear();
        }
    }
}